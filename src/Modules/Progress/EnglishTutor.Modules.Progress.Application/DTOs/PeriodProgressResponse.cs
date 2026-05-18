namespace EnglishTutor.Modules.Progress.Application.DTOs;

public sealed record WeeklyProgressResponse(
    Guid UserId,
    string TargetLanguageCode,
    int Year,
    int WeekNumber,
    int ExpEarned,
    int ActivityCount);

public sealed record MonthlyProgressResponse(
    Guid UserId,
    string TargetLanguageCode,
    int Year,
    int Month,
    int ExpEarned,
    int ActivityCount);
