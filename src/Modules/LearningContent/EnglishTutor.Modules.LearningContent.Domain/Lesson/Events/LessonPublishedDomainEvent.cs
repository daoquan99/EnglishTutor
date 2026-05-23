using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.LearningContent.Domain.Lesson.Events;

public sealed record LessonPublishedDomainEvent(
    Guid LessonId,
    string TargetLanguageCode,
    string Level,
    string Topic,
    string Skill,
    DateTime PublishedAtUtc) : DomainEvent;
