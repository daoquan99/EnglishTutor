namespace EnglishTutor.Modules.Mistakes.Application.Shared.DTOs;

public sealed record MistakeResponse(
    Guid Id,
    string TargetLanguageCode,
    string Type,
    string Category,
    string OriginalText,
    string CorrectedText,
    string Explanation,
    string SourceType,
    Guid SourceId,
    string Status,
    DateTime NextReviewAtUtc,
    DateTime CreatedAtUtc);
