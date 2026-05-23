using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.Modules.Assessments.Domain.Shared;

namespace EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Events;

public sealed record AssessmentStartedDomainEvent(
    Guid UserId,
    Guid AssessmentAttemptId,
    Guid AssessmentDefinitionId,
    string TargetLanguageCode,
    string CurrentLevel,
    DateTime StartedAtUtc) : DomainEvent;

public sealed record AssessmentPassedDomainEvent(
    Guid UserId,
    Guid AssessmentAttemptId,
    Guid AssessmentDefinitionId,
    string TargetLanguageCode,
    string CurrentLevel,
    int TotalScore,
    IReadOnlyDictionary<AssessmentSkill, int> SectionScores,
    DateTime PassedAtUtc) : DomainEvent;

public sealed record AssessmentFailedDomainEvent(
    Guid UserId,
    Guid AssessmentAttemptId,
    Guid AssessmentDefinitionId,
    string TargetLanguageCode,
    string CurrentLevel,
    int TotalScore,
    IReadOnlyDictionary<AssessmentSkill, int> SectionScores,
    DateTime FailedAtUtc) : DomainEvent;
