namespace EnglishTutor.Modules.Vocabulary.Application.Shared.DTOs;

public sealed record ReviewResultResponse(
    Guid VocabularyItemId,
    string MasteryStatus,
    int MeaningMasteryScore,
    int PronunciationMasteryScore,
    int ExampleSentenceScore,
    int ReviewCount,
    int ConsecutiveCorrectCount,
    bool CanMarkMastered,
    DateTime NextReviewAtUtc);
