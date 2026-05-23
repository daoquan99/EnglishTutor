using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Entities;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Enums;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Events;

namespace EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;

public sealed class UserLearningPathCard : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public ContentType ContentType { get; private set; }
    public Guid ContentId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Level { get; private set; } = string.Empty;
    public string? Skill { get; private set; }
    public string? Topic { get; private set; }
    public LearningPathCardStatus Status { get; private set; }
    public int Order { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime LastUpdatedAtUtc { get; private set; }

    private UserLearningPathCard() { }

    public static UserLearningPathCard Create(
        Guid userId,
        string targetLanguageCode,
        ContentType contentType,
        Guid contentId,
        string title,
        string level,
        string? skill,
        string? topic,
        LearningPathCardStatus status,
        int order,
        DateTime utcNow)
    {
        if (userId == Guid.Empty || contentId == Guid.Empty)
        {
            throw new DomainException("User id and content id are required.");
        }

        return new UserLearningPathCard
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TargetLanguageCode = LessonTranslation.NormalizeLanguage(targetLanguageCode),
            ContentType = contentType,
            ContentId = contentId,
            Title = LessonTranslation.NormalizeRequired(title, 200, "Learning path title"),
            Level = LessonTranslation.NormalizeRequired(level, 10, "Level"),
            Skill = string.IsNullOrWhiteSpace(skill) ? null : skill.Trim(),
            Topic = string.IsNullOrWhiteSpace(topic) ? null : topic.Trim(),
            Status = status,
            Order = order,
            LastUpdatedAtUtc = utcNow,
            CreatedAtUtc = utcNow
        };
    }

    public void MarkInProgress(DateTime utcNow)
    {
        if (Status == LearningPathCardStatus.Available)
        {
            Status = LearningPathCardStatus.InProgress;
            LastUpdatedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }
    }

    public void Unlock(DateTime utcNow)
    {
        if (Status == LearningPathCardStatus.Locked)
        {
            Status = LearningPathCardStatus.Available;
            LastUpdatedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }
    }

    public void Lock(DateTime utcNow)
    {
        if (Status != LearningPathCardStatus.Completed)
        {
            Status = LearningPathCardStatus.Locked;
            LastUpdatedAtUtc = utcNow;
            UpdatedAtUtc = utcNow;
        }
    }

    public void MarkCompleted(int durationSeconds, DateTime utcNow)
    {
        if (Status == LearningPathCardStatus.Completed)
        {
            return;
        }

        Status = LearningPathCardStatus.Completed;
        CompletedAtUtc = utcNow;
        LastUpdatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        if (ContentType == ContentType.Lesson)
        {
            AddDomainEvent(new LessonCompletedDomainEvent(
                UserId,
                ContentId,
                TargetLanguageCode,
                Level,
                Topic ?? string.Empty,
                Skill ?? string.Empty,
                Math.Max(durationSeconds, 0),
                utcNow));
        }
        else if (ContentType == ContentType.ConversationScenario)
        {
            AddDomainEvent(new ConversationScenarioCompletedDomainEvent(
                UserId,
                ContentId,
                TargetLanguageCode,
                Level,
                Math.Max(durationSeconds, 0),
                utcNow));
        }
    }
}
