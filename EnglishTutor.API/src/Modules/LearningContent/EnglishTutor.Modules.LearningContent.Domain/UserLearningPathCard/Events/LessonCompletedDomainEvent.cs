using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Events;

public sealed record LessonCompletedDomainEvent(
    Guid UserId,
    Guid LessonId,
    string TargetLanguageCode,
    string Level,
    string Topic,
    string Skill,
    int DurationSeconds,
    DateTime CompletedAtUtc) : DomainEvent;
