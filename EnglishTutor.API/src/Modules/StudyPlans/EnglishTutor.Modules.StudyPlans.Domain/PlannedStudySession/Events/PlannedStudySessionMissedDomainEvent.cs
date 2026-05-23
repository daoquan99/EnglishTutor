using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.StudyPlans.Domain.Events;

public sealed record PlannedStudySessionMissedDomainEvent(
    Guid UserId,
    Guid SessionId,
    string TargetLanguageCode,
    DateTime ScheduledDateUtc,
    DateTime MissedAtUtc) : DomainEvent;
