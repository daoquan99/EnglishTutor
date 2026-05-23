using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;

public sealed record StudyPlanUpdatedIntegrationEvent(
    Guid UserId,
    Guid StudyPlanId,
    string TargetLanguageCode,
    DateTime UpdatedAtUtc) : IntegrationEvent;
