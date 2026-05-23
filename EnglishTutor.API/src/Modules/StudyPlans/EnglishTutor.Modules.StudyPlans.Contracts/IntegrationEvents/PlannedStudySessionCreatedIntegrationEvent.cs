using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;

public sealed record PlannedStudySessionCreatedIntegrationEvent(
    Guid UserId,
    Guid StudyPlanId,
    Guid SessionId,
    string TargetLanguageCode,
    DateTime ScheduledDateUtc,
    DateTime CreatedAtUtc) : IntegrationEvent;
