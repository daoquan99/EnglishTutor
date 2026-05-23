namespace EnglishTutor.Modules.AI.Application.Commands.CorrectSentence;

public sealed record SentenceCorrectionResult(
    string OriginalText,
    string CorrectedText,
    string NaturalVersion,
    int GrammarScore,
    int VocabularyScore,
    string Feedback,
    IReadOnlyList<MistakeDetail> Mistakes);
