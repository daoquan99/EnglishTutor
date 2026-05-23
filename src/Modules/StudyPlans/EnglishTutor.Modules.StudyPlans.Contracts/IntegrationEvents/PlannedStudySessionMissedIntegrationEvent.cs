using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;

public sealed record PlannedStudySessionMissedIntegrationEvent(
    Guid UserId,
    Guid SessionId,
    string TargetLanguageCode,
    DateTime ScheduledDateUtc,
    DateTime MissedAtUtc) : IntegrationEvent;
