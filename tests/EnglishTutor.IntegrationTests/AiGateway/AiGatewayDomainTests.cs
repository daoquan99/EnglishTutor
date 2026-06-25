using System;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.IntegrationTests.AiGateway;

public class AiGatewayDomainTests
{
    [Fact]
    public void AiProvider_Create_ShouldInitializeCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var provider = AiProvider.Create(id, "Google Gemini", "GOOGLE", true);

        // Assert
        provider.Id.Should().Be(id);
        provider.Name.Should().Be("Google Gemini");
        provider.Code.Should().Be("google");
        provider.IsActive.Should().BeTrue();
        provider.Version.Should().Be(1);
    }

    [Fact]
    public void AiModel_Create_ShouldInitializeCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var capabilities = new[] { "roleplay", "shadowing" };

        // Act
        var model = AiModel.Create(id, providerId, "Gemini 1.5 Pro", "Gemini-1.5-Pro", capabilities, true);

        // Assert
        model.Id.Should().Be(id);
        model.ProviderId.Should().Be(providerId);
        model.Code.Should().Be("gemini-1.5-pro");
        model.Capabilities.Should().BeEquivalentTo(capabilities);
        model.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AiProviderKey_SetCooldown_ShouldSetCooldownTimestamp()
    {
        // Arrange
        var key = AiProviderKey.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "Test Key", 
            "encrypted-data", 
            "gsk_••••1234", 
            1, 
            true);

        // Act
        key.SetCooldown(TimeSpan.FromMinutes(5));

        // Assert
        key.CooldownUntilUtc.Should().NotBeNull();
        key.CooldownUntilUtc.Value.Should().BeAfter(DateTime.UtcNow);
        key.IsOnCooldown(DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void AiProviderKey_Quarantine_ShouldDeactivateAndClearCooldown()
    {
        // Arrange
        var key = AiProviderKey.Create(
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            "Test Key", 
            "encrypted-data", 
            "gsk_••••1234", 
            1, 
            true);
        key.SetCooldown(TimeSpan.FromMinutes(5));

        // Act
        key.Quarantine();

        // Assert
        key.IsActive.Should().BeFalse();
        key.CooldownUntilUtc.Should().BeNull();
        key.IsOnCooldown(DateTime.UtcNow).Should().BeFalse();
    }

    [Fact]
    public void AiRouteLease_Confirm_WhenReserved_ShouldSucceed()
    {
        // Arrange
        var lease = AiRouteLease.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(10),
            "idem-key-1");

        // Act
        lease.Confirm();

        // Assert
        lease.Status.Should().Be(AiRouteLeaseStatus.Confirmed);
    }

    [Fact]
    public void AiRouteLease_Confirm_WhenAlreadyReleased_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var lease = AiRouteLease.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddMinutes(10),
            "idem-key-2");
        lease.Release();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => lease.Confirm());
    }
}
