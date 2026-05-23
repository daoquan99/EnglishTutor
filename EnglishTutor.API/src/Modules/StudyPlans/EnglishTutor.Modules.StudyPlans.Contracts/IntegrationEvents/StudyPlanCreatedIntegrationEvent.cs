using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;

public sealed record StudyPlanCreatedIntegrationEvent(
    Guid UserId,
    Guid StudyPlanId,
    string TargetLanguageCode,
    DateTime CreatedAtUtc) : IntegrationEvent;
