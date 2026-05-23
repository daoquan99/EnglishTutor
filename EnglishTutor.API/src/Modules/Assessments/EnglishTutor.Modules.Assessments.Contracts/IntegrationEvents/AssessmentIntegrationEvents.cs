using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Assessments.Contracts.IntegrationEvents;

public sealed record AssessmentStartedIntegrationEvent(
    Guid UserId,
    Guid AssessmentAttemptId,
    Guid AssessmentDefinitionId,
    string TargetLanguageCode,
    string CurrentLevel,
    DateTime StartedAtUtc) : IntegrationEvent;

public sealed record AssessmentCompletedIntegrationEvent(
    Guid UserId,
    Guid AssessmentAttemptId,
    Guid AssessmentDefinitionId,
    string TargetLanguageCode,
    string AssessmentType,
    int Score,
    bool IsPassed,
    DateTime CompletedAtUtc) : IntegrationEvent;

public sealed record AssessmentPassedIntegrationEvent(
    Guid UserId,
    Guid AssessmentAttemptId,
    Guid AssessmentDefinitionId,
    string TargetLanguageCode,
    int Score,
    DateTime PassedAtUtc) : IntegrationEvent;

public sealed record AssessmentFailedIntegrationEvent(
    Guid UserId,
    Guid AssessmentAttemptId,
    Guid AssessmentDefinitionId,
    string TargetLanguageCode,
    int Score,
    IReadOnlyList<string> WeakSkills,
    DateTime FailedAtUtc) : IntegrationEvent;

public sealed record LevelUpApprovedIntegrationEvent(
    Guid UserId,
    string TargetLanguageCode,
    string PreviousLevel,
    string NewLevel,
    int AssessmentScore,
    Guid AssessmentAttemptId,
    DateTime ApprovedAtUtc) : IntegrationEvent;
