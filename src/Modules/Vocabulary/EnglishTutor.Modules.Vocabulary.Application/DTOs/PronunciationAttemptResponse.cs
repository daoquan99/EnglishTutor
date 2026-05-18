namespace EnglishTutor.Modules.Vocabulary.Application.DTOs;

public sealed record PronunciationAttemptResponse(
    Guid AttemptId,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    int? CompletenessScore,
    DateTime AttemptedAtUtc);
