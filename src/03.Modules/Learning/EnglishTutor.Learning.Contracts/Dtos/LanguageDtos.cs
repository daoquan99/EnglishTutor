namespace EnglishTutor.Learning.Contracts.Dtos;

public sealed record LanguageContextDto(
    Guid LanguagePairId,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    long Version);

public sealed record LanguagePairDto(
    Guid Id,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    string Status,
    long Version);
