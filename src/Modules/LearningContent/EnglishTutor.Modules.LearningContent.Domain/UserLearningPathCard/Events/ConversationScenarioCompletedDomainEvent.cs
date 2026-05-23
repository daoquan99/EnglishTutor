using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Events;

public sealed record ConversationScenarioCompletedDomainEvent(
    Guid UserId,
    Guid ConversationScenarioId,
    string TargetLanguageCode,
    string Level,
    int DurationSeconds,
    DateTime CompletedAtUtc) : DomainEvent;
