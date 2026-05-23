namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetDeadLetters;

public sealed record DeadLetterMessageResponse(
    Guid Id, Guid EventId, string EventType, string SourceModule,
    DateTime FailedAtUtc, int RetryCount, string LastError,
    string? StackTrace, string Status);
