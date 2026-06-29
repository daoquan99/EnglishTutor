using System;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;
using EnglishTutor.AiGateway.Domain.Aggregates.AiVoice;
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
        var id = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var capabilities = new[]
        {
            AiModelCapability.ContentGeneration,
            AiModelCapability.LiveConversation
        };

        var model = AiModel.Create(
            id,
            providerId,
            "Gemini 3.1 Flash Live",
            "gemini-3.1-flash-live",
            "models/gemini-3.1-flash-live-preview",
            capabilities,
            true,
            AiModelLifecycle.Preview,
            true);

        model.Id.Should().Be(id);
        model.ProviderId.Should().Be(providerId);
        model.Code.Should().Be("gemini-3.1-flash-live");
        model.ProviderModelId.Should().Be("gemini-3.1-flash-live-preview");
        model.Capabilities.Should().BeEquivalentTo(capabilities);
        model.ThinkingEnabled.Should().BeTrue();
        model.Lifecycle.Should().Be(AiModelLifecycle.Preview);
        model.IsActive.Should().BeTrue();
    }

    [Fact]
    public void AiModel_SetVoices_ShouldRequireLiveConversationAndSingleDefault()
    {
        var model = AiModel.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Gemini Live",
            "gemini-live",
            "gemini-live",
            [AiModelCapability.LiveConversation],
            false,
            AiModelLifecycle.Preview,
            true);
        var firstVoiceId = Guid.NewGuid();
        var secondVoiceId = Guid.NewGuid();

        model.SetVoices([firstVoiceId, secondVoiceId], secondVoiceId);

        model.ModelVoices.Should().HaveCount(2);
        model.ModelVoices.Single(x => x.IsDefault).VoiceId.Should().Be(secondVoiceId);
    }

    [Fact]
    public void AiModel_Create_ShouldRejectActiveDeprecatedModel()
    {
        var action = () => AiModel.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Deprecated",
            "deprecated",
            "deprecated",
            [AiModelCapability.ContentGeneration],
            null,
            AiModelLifecycle.Deprecated,
            true);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AiVoice_Create_ShouldNormalizeProviderVoiceId()
    {
        var voice = AiVoice.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Zephyr",
            "Zephyr",
            "Bright",
            AiVoiceGender.Female,
            true);

        voice.VoiceId.Should().Be("zephyr");
        voice.Style.Should().Be("Bright");
        voice.Gender.Should().Be(AiVoiceGender.Female);
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
