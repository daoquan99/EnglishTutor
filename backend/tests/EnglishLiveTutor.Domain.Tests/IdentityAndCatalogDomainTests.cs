using EnglishLiveTutor.Domain.Common.Abstractions;
using EnglishLiveTutor.Domain.Identity;
using EnglishLiveTutor.Domain.Prompts;
using EnglishLiveTutor.Domain.TeacherVoices;
using Xunit;

namespace EnglishLiveTutor.Domain.Tests;

public sealed class IdentityAndCatalogDomainTests
{
    [Fact]
    public void IdentityAggregates_HaveAuditAndLogicalDeletion()
    {
        var user = new UserEntity("  Learner@Example.com ", "Learner", "hashed");
        Assert.Equal("LEARNER@EXAMPLE.COM", user.NormalizedEmail);
        Assert.IsAssignableFrom<IAuditable>(user);
        Assert.IsAssignableFrom<ISoftDeletable>(user);
        user.SetEnabled(false);
        Assert.False(user.IsEnabled);
        var role = new RoleEntity("Teacher", "Teaches English");
        Assert.IsAssignableFrom<ISoftDeletable>(role);
    }

    [Fact]
    public void RefreshToken_Revoke_IsIdempotent()
    {
        var now = DateTimeOffset.UtcNow;
        var refresh = new RefreshTokenEntity(Guid.NewGuid(), "HASH", now.AddDays(1));
        refresh.Revoke(now);
        refresh.Revoke(now.AddDays(1));
        Assert.Equal(now, refresh.RevokedAt);
    }

    [Fact]
    public void Prompt_VersionIncrements_WhenUpdated()
    {
        var prompt = new AiPromptTemplateEntity("cefr.A1", "CEFR", "Original");
        prompt.Update("Updated A1 prompt", false);
        Assert.Equal(2, prompt.Version);
        Assert.Equal("Updated A1 prompt", prompt.Content);
        Assert.False(prompt.IsEnabled);
        Assert.Throws<ArgumentException>(() => prompt.Update("", true));
    }

    [Fact]
    public void TeacherVoice_StoresGoogleCodeAndRecommendation()
    {
        var voice = new TeacherVoiceEntity("Aoede", "Female", "Breezy", "Warm voice",
            "Aoede (Nữ)", "Ấm áp", 1, true);
        Assert.Equal("Aoede", voice.Code);
        Assert.True(voice.IsRecommended);
        voice.SetEnabled(false);
        Assert.False(voice.IsEnabled);
    }
}
