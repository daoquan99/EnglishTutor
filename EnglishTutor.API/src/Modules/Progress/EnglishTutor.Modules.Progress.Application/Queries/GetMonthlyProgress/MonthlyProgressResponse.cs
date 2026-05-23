namespace EnglishTutor.Modules.Progress.Application.Queries.GetMonthlyProgress;

public sealed record MonthlyProgressResponse(
    Guid UserId,
    string TargetLanguageCode,
    int Year,
    int Month,
    int ExpEarned,
    int ActivityCount);
