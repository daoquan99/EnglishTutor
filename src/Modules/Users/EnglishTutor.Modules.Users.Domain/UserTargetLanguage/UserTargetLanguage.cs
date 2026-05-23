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
    private UserTargetLanguage() { }

    public static UserTargetLanguage Create(
        Guid userId,
        LanguageCode targetLanguageCode,
        LanguageLevel currentLevel,
        LanguageLevel targetLevel,
        DateTime utcNow) =>
        CreateInternal(userId, targetLanguageCode, currentLevel, targetLevel, isActive: false, utcNow);

    public static UserTargetLanguage CreateActive(
        Guid userId,
        LanguageCode targetLanguageCode,
        LanguageLevel currentLevel,
        LanguageLevel targetLevel,
        IEnumerable<UserTargetLanguage> existingTargetLanguages,
        DateTime utcNow)
    {
        foreach (var targetLanguage in existingTargetLanguages.Where(language => language.UserId == userId))
        {
            targetLanguage.Deactivate(utcNow);
        }

        return CreateInternal(userId, targetLanguageCode, currentLevel, targetLevel, isActive: true, utcNow);
    }

    private static UserTargetLanguage CreateInternal(
        Guid userId,
        LanguageCode targetLanguageCode,
        LanguageLevel currentLevel,
        LanguageLevel targetLevel,
        bool isActive,
        DateTime utcNow)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("User id is required.");
        }

        var targetLanguage = new UserTargetLanguage
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            CurrentLevel = currentLevel,
            TargetLevel = targetLevel,
            IsActive = isActive,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };

        targetLanguage.AddDomainEvent(new UserTargetLanguageAddedDomainEvent(
            userId,
            targetLanguage.TargetLanguageCode.Value,
            currentLevel.ToString(),
            targetLevel.ToString(),
            targetLanguage.IsActive));

        return targetLanguage;
    }

    public void Activate(IEnumerable<UserTargetLanguage> userTargetLanguages, DateTime utcNow)
    {
        foreach (var targetLanguage in userTargetLanguages.Where(language => language.UserId == UserId && language.Id != Id))
        {
            targetLanguage.Deactivate(utcNow);
        }

        if (IsActive)
        {
            return;
        }

        IsActive = true;
        UpdatedAtUtc = utcNow;
        AddActivationChangedEvent(utcNow);
    }

    public void Deactivate(DateTime utcNow)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        UpdatedAtUtc = utcNow;
        AddActivationChangedEvent(utcNow);
    }

    public void UpdateLevel(LanguageLevel newLevel, DateTime utcNow)
    {
        if (newLevel == CurrentLevel)
        {
            return;
        }

        var previousLevel = CurrentLevel;
        CurrentLevel = newLevel;
        UpdatedAtUtc = utcNow;

        AddDomainEvent(new UserLevelChangedDomainEvent(
            UserId,
            TargetLanguageCode.Value,
            previousLevel.ToString(),
            newLevel.ToString(),
            UpdatedAtUtc));
    }

    private void AddActivationChangedEvent(DateTime utcNow) =>
        AddDomainEvent(new UserTargetLanguageActivationChangedDomainEvent(
            UserId,
            TargetLanguageCode.Value,
            CurrentLevel.ToString(),
            TargetLevel.ToString(),
            IsActive,
            utcNow));
}
