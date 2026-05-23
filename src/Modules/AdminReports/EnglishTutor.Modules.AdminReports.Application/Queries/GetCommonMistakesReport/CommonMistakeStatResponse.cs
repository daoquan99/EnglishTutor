namespace EnglishTutor.Modules.AdminReports.Application.Queries.GetCommonMistakesReport;

public sealed record CommonMistakeStatResponse(
    string TargetLanguageCode, string MistakeType, string Category,
    int OccurrenceCount, int AffectedUsers,
    string ExampleOriginal, string ExampleCorrected, DateTime LastUpdatedAtUtc);
