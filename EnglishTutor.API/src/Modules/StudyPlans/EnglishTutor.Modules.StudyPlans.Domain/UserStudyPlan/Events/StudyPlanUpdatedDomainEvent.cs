using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.StudyPlans.Domain.Events;

public sealed record StudyPlanUpdatedDomainEvent(
    Guid UserId,
    Guid StudyPlanId,
    string TargetLanguageCode,
    DateTime UpdatedAtUtc) : DomainEvent;
