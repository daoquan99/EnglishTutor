namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetTodayVocabulary;

public sealed record TodayVocabularyResponse(
    IReadOnlyList<TodayVocabularyItemResponse> NewItems,
    IReadOnlyList<TodayVocabularyItemResponse> ReviewItems,
    int NewWordsCount,
    int ReviewWordsCount,
    int NewWordsPerDay);

public sealed record TodayVocabularyItemResponse(
    Guid VocabularyItemId,
    string Word,
    string? Phonetic,
    string MasteryStatus,
    int MeaningMasteryScore,
    int ReviewCount,
    DateTime NextReviewAtUtc);
