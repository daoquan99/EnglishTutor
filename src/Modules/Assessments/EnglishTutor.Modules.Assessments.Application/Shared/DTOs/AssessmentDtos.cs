namespace EnglishTutor.Modules.Assessments.Application.Shared.DTOs;

public sealed record AttemptDetailResponse(
    Guid AttemptId,
    Guid AssessmentDefinitionId,
    string TargetLanguageCode,
    string CurrentLevel,
    string Status,
    IReadOnlyList<AssessmentSectionResponse> Sections);

public sealed record AssessmentSectionResponse(
    Guid Id,
    string Skill,
    string Title,
    decimal Weight,
    int Order,
    IReadOnlyList<AssessmentQuestionResponse> Questions);

public sealed record AssessmentQuestionResponse(
    Guid Id,
    string Prompt,
    string QuestionType,
    bool IsAiGraded,
    int MaxScore,
    int Order,
    string? Explanation);

public sealed record AttemptResultResponse(
    Guid AttemptId,
    string Status,
    int TotalScore,
    bool IsPassed,
    IReadOnlyList<SectionScoreResponse> SectionScores,
    DateTime? GradedAtUtc);

public sealed record SectionScoreResponse(string Skill, int Score);
