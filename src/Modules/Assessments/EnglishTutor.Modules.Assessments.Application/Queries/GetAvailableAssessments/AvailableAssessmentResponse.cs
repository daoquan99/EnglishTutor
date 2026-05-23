namespace EnglishTutor.Modules.Assessments.Application.Queries.GetAvailableAssessments;

public sealed record AvailableAssessmentResponse(
    Guid Id,
    string AssessmentType,
    string TargetLanguageCode,
    string ForLevel,
    string Title,
    string? Description,
    int PassingScore,
    int MinSkillScore,
    int? TimeLimitMinutes);
