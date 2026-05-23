namespace EnglishTutor.Modules.Vocabulary.Application.Shared.DTOs;

public sealed record ReviewResultResponse(
    Guid VocabularyItemId,
    string MasteryStatus,
    int MeaningMasteryScore,
    DateTime NextReviewAtUtc);
