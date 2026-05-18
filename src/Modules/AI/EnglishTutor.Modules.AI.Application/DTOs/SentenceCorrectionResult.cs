namespace EnglishTutor.Modules.AI.Application.DTOs;

public sealed record SentenceCorrectionResult(
    string OriginalText,
    string CorrectedText,
    string NaturalVersion,
    int GrammarScore,
    int VocabularyScore,
    string Feedback,
    IReadOnlyList<MistakeDetail> Mistakes);
