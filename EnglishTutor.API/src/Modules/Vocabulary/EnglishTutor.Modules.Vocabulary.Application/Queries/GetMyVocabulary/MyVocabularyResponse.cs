namespace EnglishTutor.Modules.Vocabulary.Application.Queries.GetMyVocabulary;

public sealed record MyVocabularyResponse(
    IReadOnlyList<MyVocabularyItemResponse> Items,
    MyVocabularyCountsResponse Counts);

public sealed record MyVocabularyItemResponse(
    Guid VocabularyItemId,
    string Word,
    string? Phonetic,
    string MasteryStatus,
    int MeaningMasteryScore,
    int ReviewCount,
    DateTime? NextReviewAtUtc);

public sealed record MyVocabularyCountsResponse(
    int New,
    int Learning,
    int Reviewing,
    int Weak,
    int Mastered,
    int Total);
