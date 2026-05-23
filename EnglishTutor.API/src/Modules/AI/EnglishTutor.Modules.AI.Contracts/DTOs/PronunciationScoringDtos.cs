namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record PronunciationScoringRequest(
    Guid UserId,
    string ExpectedText,
    string LanguageCode,
    Stream AudioStream,
    string ContentType);

public sealed record PronunciationScoringResponse(
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    int CompletenessScore,
    string RecognizedText,
    string Feedback,
    string? WordLevelFeedbackJson);
