using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;

using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.AiGateway;

[Collection("EnglishTutorIntegrationTests")]
public class AiGatewayPersistenceTests
{
    private sealed class AiGatewayTestFactory : IntegrationTestFactory
    {
    }

    [Fact]
    public async Task Creating_AiGateway_Entities_Should_Stamp_Audit_Fields()
    {
        // Arrange
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        var aiDb = scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>();
        await aiDb.Database.MigrateAsync();

        var providerId = Guid.NewGuid();
        var provider = AiProvider.Create(providerId, "OpenAI", "openai", true);

        // Act
        await aiDb.Providers.AddAsync(provider);
        await aiDb.SaveChangesAsync();

        // Assert
        var savedProvider = await aiDb.Providers.FindAsync(providerId);
        savedProvider.Should().NotBeNull();
        savedProvider!.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        savedProvider.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Deleting_AiGateway_Entities_Should_Soft_Delete_And_Apply_Query_Filters()
    {
        // Arrange
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        var aiDb = scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>();
        await aiDb.Database.MigrateAsync();

        var providerId = Guid.NewGuid();
        var provider = AiProvider.Create(
            providerId,
            "Google Gemini Test",
            $"google-test-{providerId:N}",
            true);
        await aiDb.Providers.AddAsync(provider);
        await aiDb.SaveChangesAsync();

        // Act
        aiDb.Providers.Remove(provider);
        await aiDb.SaveChangesAsync();

        // Assert: Excluded by default query filter
        var queriedProvider = await aiDb.Providers.FirstOrDefaultAsync(p => p.Id == providerId);
        queriedProvider.Should().BeNull();

        // Accessible when ignoring query filters
        var softDeletedProvider = await aiDb.Providers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == providerId);

        softDeletedProvider.Should().NotBeNull();
        softDeletedProvider!.IsDeleted.Should().BeTrue();
        softDeletedProvider.DeletedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Duplicate_UserId_And_IdempotencyKey_Should_Throw_DbUpdateException()
    {
        // Arrange
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        var aiDb = scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>();
        await aiDb.Database.MigrateAsync();

        var userId = Guid.NewGuid();
        var routingRuleId = Guid.NewGuid();
        var modelId = Guid.NewGuid();
        var keyId = Guid.NewGuid();
        const string idempotencyKey = "unique-test-key";

        var lease1 = AiRouteLease.Create(
            Guid.NewGuid(),
            userId,
            routingRuleId,
            modelId,
            keyId,
            DateTime.UtcNow.AddMinutes(10),
            idempotencyKey);

        var lease2 = AiRouteLease.Create(
            Guid.NewGuid(),
            userId,
            routingRuleId,
            modelId,
            keyId,
            DateTime.UtcNow.AddMinutes(10),
            idempotencyKey);

        await aiDb.RouteLeases.AddAsync(lease1);
        await aiDb.SaveChangesAsync();

        // Act & Assert: adding duplicate composite key should fail
        await aiDb.RouteLeases.AddAsync(lease2);
        Func<Task> saveAction = async () => await aiDb.SaveChangesAsync();
        await saveAction.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task SecretStorageBoundary_ShouldOnlyStoreEncryptedKeyAndMask()
    {
        // Arrange
        await using var factory = new AiGatewayTestFactory();
        using var scope = factory.Services.CreateScope();
        var aiDb = scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>();
        await aiDb.Database.MigrateAsync();

        var providerId = Guid.NewGuid();
        var keyId = Guid.NewGuid();

        // Raw key must never be passed to the aggregate or database
        const string encryptedKey = "encrypted_value_xyz";
        const string keyMask = "gsk_••••••••1234";

        var key = AiProviderKey.Create(
            keyId,
            providerId,
            "Production Key",
            encryptedKey,
            keyMask,
            1,
            true);

        // Act
        await aiDb.ProviderKeys.AddAsync(key);
        await aiDb.SaveChangesAsync();

        // Assert: verify DB maps and retrieves the entity correctly
        var savedKey = await aiDb.ProviderKeys.AsNoTracking().FirstOrDefaultAsync(k => k.Id == keyId);
        savedKey.Should().NotBeNull();
        savedKey!.EncryptedKey.Should().Be(encryptedKey);
        savedKey.KeyMask.Should().Be(keyMask);

        // Reflection check to ensure no raw plaintext property fields are on the entity
        var propertyNames = typeof(AiProviderKey).GetProperties()
            .Select(p => p.Name.ToLowerInvariant())
            .ToList();

        propertyNames.Should().NotContain(name => 
            name.Contains("raw") || 
            name.Contains("plain") || 
            (name.Contains("key") && !name.Contains("encryptedkey") && !name.Contains("keymask") && !name.Contains("providerkeyid")));
    }
}
