namespace EnglishTutor.Modules.Vocabulary.Presentation.Requests;

public sealed record SubmitPronunciationAttemptRequest(
    string? AudioUrl,
    string RecognizedText,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    int CompletenessScore,
    string Feedback);
