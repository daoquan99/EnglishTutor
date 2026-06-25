using System;
using System.Linq;
using System.Threading.Tasks;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.Audit;

[Collection("EnglishTutorIntegrationTests")]
public class AuditPersistenceTests
{
    private sealed class AuditTestFactory : IntegrationTestFactory
    {
    }

    [Fact]
    public void Interceptor_Should_Be_Registered_In_DI_And_AuditDbContext()
    {
        // Arrange
        using var factory = new AuditTestFactory();
        using var scope = factory.Services.CreateScope();

        // Act & Assert: Verify it resolves from DI
        var interceptor = scope.ServiceProvider.GetService<AuditableEntitySaveChangesInterceptor>();
        interceptor.Should().NotBeNull();

        // Verify AuditDbContext options can be resolved
        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();
        auditDb.Should().NotBeNull();
    }

    [Fact]
    public async Task Saving_SecurityEvent_Should_Automatically_Stamp_Audit_Fields()
    {
        // Arrange
        await using var factory = new AuditTestFactory();
        using var scope = factory.Services.CreateScope();
        var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

        // Clean database tables from previous runs
        auditDb.SecurityEvents.RemoveRange(auditDb.SecurityEvents);
        await auditDb.SaveChangesAsync();

        var occurredAt = DateTime.UtcNow;
        var securityEvent = SecurityEvent.Create(
            categoryCode: "test_category",
            sourceModule: "test_module",
            sourceEventType: "test_type",
            userId: Guid.NewGuid(),
            sessionId: Guid.NewGuid(),
            refreshTokenFamilyId: Guid.NewGuid(),
            refreshTokenId: Guid.NewGuid(),
            reasonCode: "test_reason",
            correlationId: Guid.NewGuid(),
            causationId: Guid.NewGuid(),
            ipAddressHash: "hash123",
            userAgentHash: "hash456",
            occurredAtUtc: occurredAt
        );

        // Act
        await auditDb.SecurityEvents.AddAsync(securityEvent);
        await auditDb.SaveChangesAsync();

        // Assert
        var savedEvent = await auditDb.SecurityEvents.FindAsync(securityEvent.Id);
        savedEvent.Should().NotBeNull();
        savedEvent!.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        savedEvent.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }
}
