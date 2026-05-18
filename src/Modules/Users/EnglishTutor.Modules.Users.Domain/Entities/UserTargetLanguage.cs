using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Users.Domain.Events;

namespace EnglishTutor.Modules.Users.Domain.Entities;

public sealed class UserTargetLanguage : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public LanguageLevel CurrentLevel { get; private set; }
    public LanguageLevel TargetLevel { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private UserTargetLanguage() { }

    public static UserTargetLanguage Create(
        Guid userId,
        LanguageCode targetLanguageCode,
        LanguageLevel currentLevel,
        LanguageLevel targetLevel) =>
        CreateInternal(userId, targetLanguageCode, currentLevel, targetLevel, isActive: false);

    public static UserTargetLanguage CreateActive(
        Guid userId,
        LanguageCode targetLanguageCode,
        LanguageLevel currentLevel,
        LanguageLevel targetLevel,
        IEnumerable<UserTargetLanguage> existingTargetLanguages)
    {
        foreach (var targetLanguage in existingTargetLanguages.Where(language => language.UserId == userId))
        {
            targetLanguage.Deactivate();
        }

        return CreateInternal(userId, targetLanguageCode, currentLevel, targetLevel, isActive: true);
    }

    private static UserTargetLanguage CreateInternal(
        Guid userId,
        LanguageCode targetLanguageCode,
        LanguageLevel currentLevel,
        LanguageLevel targetLevel,
        bool isActive)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        var now = DateTime.UtcNow;
        var targetLanguage = new UserTargetLanguage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            CurrentLevel = currentLevel,
            TargetLevel = targetLevel,
            IsActive = isActive,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        targetLanguage.AddDomainEvent(new UserTargetLanguageAddedDomainEvent(
            userId,
            targetLanguage.TargetLanguageCode.Value,
            currentLevel.ToString(),
            targetLevel.ToString()));

        return targetLanguage;
    }

    public void Activate(IEnumerable<UserTargetLanguage> userTargetLanguages)
    {
        foreach (var targetLanguage in userTargetLanguages.Where(language => language.UserId == UserId))
        {
            targetLanguage.Deactivate();
        }

        IsActive = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void UpdateLevel(LanguageLevel newLevel)
    {
        if (newLevel == CurrentLevel)
        {
            return;
        }

        var previousLevel = CurrentLevel;
        CurrentLevel = newLevel;
        UpdatedAtUtc = DateTime.UtcNow;

        AddDomainEvent(new UserLevelChangedDomainEvent(
            UserId,
            TargetLanguageCode.Value,
            previousLevel.ToString(),
            newLevel.ToString(),
            UpdatedAtUtc));
    }
}
