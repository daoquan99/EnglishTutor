namespace EnglishTutor.Modules.StudyPlans.Application.Shared.DTOs;

public sealed record PlannedSessionResponse(
    Guid Id,
    Guid StudyPlanId,
    Guid UserId,
    string TargetLanguageCode,
    DateTime ScheduledDateUtc,
    string Status,
    DateTime? CompletedAtUtc,
    DateTime? MissedAtUtc);
