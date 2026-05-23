using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.StudyPlans.Domain.Events;

public sealed record StudyPlanCreatedDomainEvent(
    Guid UserId,
    Guid StudyPlanId,
    string TargetLanguageCode,
    DateTime CreatedAtUtc) : DomainEvent;
