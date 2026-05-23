using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Users.Domain.Events;

namespace EnglishTutor.Modules.Users.Domain.Entities;

public sealed class UserLanguageSettings : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode NativeLanguageCode { get; private set; } = default!;
    public LanguageCode UiLanguageCode { get; private set; } = default!;
    public LanguageCode ExplanationLanguageCode { get; private set; } = default!;
    public LanguageCode ActiveTargetLanguageCode { get; private set; } = default!;
    private UserLanguageSettings() { }

    public static UserLanguageSettings CreateDefault(Guid userId, DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        return new UserLanguageSettings
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NativeLanguageCode = LanguageCode.Vietnamese,
            UiLanguageCode = LanguageCode.Vietnamese,
            ExplanationLanguageCode = LanguageCode.Vietnamese,
            ActiveTargetLanguageCode = LanguageCode.English,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };
    }

    public void Update(
        LanguageCode nativeLanguageCode,
        LanguageCode uiLanguageCode,
        LanguageCode explanationLanguageCode,
        LanguageCode activeTargetLanguageCode,
        DateTime utcNow)
    {
        NativeLanguageCode = nativeLanguageCode ?? throw new DomainException("Native language code is required.");
        UiLanguageCode = uiLanguageCode ?? throw new DomainException("UI language code is required.");
        ExplanationLanguageCode = explanationLanguageCode ?? throw new DomainException("Explanation language code is required.");
        ActiveTargetLanguageCode = activeTargetLanguageCode ?? throw new DomainException("Active target language code is required.");
        UpdatedAtUtc = utcNow;

        AddDomainEvent(new UserLanguageSettingsUpdatedDomainEvent(
            UserId,
            NativeLanguageCode.Value,
            UiLanguageCode.Value,
            ExplanationLanguageCode.Value,
            ActiveTargetLanguageCode.Value));
    }
}
