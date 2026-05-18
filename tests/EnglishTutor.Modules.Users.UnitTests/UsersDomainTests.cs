using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Users.Application.Commands.UpdateLanguageSettings;
using EnglishTutor.Modules.Users.Application.Commands.UpdateUserProfile;
using EnglishTutor.Modules.Users.Domain.Entities;
using EnglishTutor.Modules.Users.Domain.Events;
using Xunit;

namespace EnglishTutor.Modules.Users.UnitTests;

public sealed class UsersDomainTests
{
    [Fact]
    public void UserProfile_Create_Raises_ProfileCreated_Event()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "Learner");

        Assert.Equal("Learner", profile.DisplayName.Value);
        Assert.Contains(profile.DomainEvents, domainEvent => domainEvent is UserProfileCreatedDomainEvent);
    }

    [Fact]
    public void UserProfile_Update_Rejects_Too_Long_Bio()
    {
        var profile = UserProfile.Create(Guid.NewGuid(), "Learner");

        Assert.Throws<DomainException>(() => profile.UpdateProfile("Learner", null, new string('x', 501)));
    }

    [Fact]
    public void LanguageSettings_Default_Uses_Vietnamese_And_English_Target()
    {
        var settings = UserLanguageSettings.CreateDefault(Guid.NewGuid());

        Assert.Equal("vi", settings.NativeLanguageCode.Value);
        Assert.Equal("en", settings.ActiveTargetLanguageCode.Value);
    }

    [Fact]
    public void LanguageSettings_Update_Raises_Event()
    {
        var settings = UserLanguageSettings.CreateDefault(Guid.NewGuid());
        settings.ClearDomainEvents();

        settings.Update(LanguageCode.English, LanguageCode.English, LanguageCode.English, LanguageCode.Vietnamese);

        Assert.Contains(settings.DomainEvents, domainEvent => domainEvent is UserLanguageSettingsUpdatedDomainEvent);
    }

    [Fact]
    public void CreateActive_TargetLanguage_Deactivates_Existing_Active_Languages()
    {
        var userId = Guid.NewGuid();
        var existing = UserTargetLanguage.CreateActive(
            userId,
            LanguageCode.English,
            LanguageLevel.A1,
            LanguageLevel.B2,
            []);

        var next = UserTargetLanguage.CreateActive(
            userId,
            LanguageCode.Vietnamese,
            LanguageLevel.A1,
            LanguageLevel.B1,
            [existing]);

        Assert.False(existing.IsActive);
        Assert.True(next.IsActive);
    }

    [Fact]
    public void TargetLanguage_UpdateLevel_Raises_LevelChanged_Event()
    {
        var target = UserTargetLanguage.Create(Guid.NewGuid(), LanguageCode.English, LanguageLevel.A1, LanguageLevel.B2);
        target.ClearDomainEvents();

        target.UpdateLevel(LanguageLevel.A2);

        Assert.Equal(LanguageLevel.A2, target.CurrentLevel);
        Assert.Contains(target.DomainEvents, domainEvent => domainEvent is UserLevelChangedDomainEvent);
    }

    [Fact]
    public void UserProfile_Validator_Rejects_Empty_DisplayName()
    {
        var validator = new UpdateUserProfileCommandValidator();

        var result = validator.Validate(new UpdateUserProfileCommand(Guid.NewGuid(), "", null, null));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void LanguageSettings_Validator_Rejects_Invalid_Language_Code()
    {
        var validator = new UpdateLanguageSettingsCommandValidator();

        var result = validator.Validate(new UpdateLanguageSettingsCommand(Guid.NewGuid(), "english", "vi", "vi", "en"));

        Assert.False(result.IsValid);
    }
}
