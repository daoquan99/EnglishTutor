namespace EnglishTutor.Modules.Progress.Application.Queries.GetActivities;

public sealed record ActivityLogResponse(
    Guid ActivityId,
    string ActivityType,
    DateTime CompletedAtUtc,
    int DurationSeconds,
    int ExpEarned,
    int Score,
    string Result);
