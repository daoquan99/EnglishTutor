namespace EnglishTutor.Learning.Application.LanguagePairs;

public sealed record LanguageDefinitionView(
    Guid Id,
    string Code,
    string EnglishName,
    string NativeName,
    bool IsAvailableAsNative,
    bool IsAvailableAsTarget,
    int SortOrder);

public sealed record LearnerLanguagePairView(
    Guid Id,
    string NativeLanguageCode,
    string TargetLanguageCode,
    string ExplanationLanguageCode,
    string Status,
    long Version);
