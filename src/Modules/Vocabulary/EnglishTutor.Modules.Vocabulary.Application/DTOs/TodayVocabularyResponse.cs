namespace EnglishTutor.Modules.Vocabulary.Application.DTOs;

public sealed record TodayVocabularyResponse(IReadOnlyList<TodayVocabularyItemResponse> Items);

public sealed record TodayVocabularyItemResponse(
    Guid VocabularyItemId,
    string Word,
    string MasteryStatus,
    DateTime NextReviewAtUtc);
