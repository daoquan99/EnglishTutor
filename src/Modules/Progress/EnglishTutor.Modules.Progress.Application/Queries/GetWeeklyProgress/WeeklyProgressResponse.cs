namespace EnglishTutor.Modules.Progress.Application.Queries.GetWeeklyProgress;

public sealed record WeeklyProgressResponse(
    Guid UserId,
    string TargetLanguageCode,
    int Year,
    int WeekNumber,
    int ExpEarned,
    int ActivityCount);
