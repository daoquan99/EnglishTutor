namespace EnglishTutor.Modules.Vocabulary.Presentation.Requests;

public sealed record SubmitExamplePronunciationRequest(
    string RecognizedText,
    int PronunciationScore,
    int AccuracyScore,
    int FluencyScore,
    string Feedback);
