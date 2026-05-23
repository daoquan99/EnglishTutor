namespace EnglishTutor.Modules.AI.Contracts.DTOs;

public sealed record CorrectionResponse(
    string CorrectedText,
    string NaturalVersion,
    int GrammarScore,
    int VocabularyScore,
    string Feedback,
    string FeedbackLanguageCode,
    IReadOnlyList<MistakeDetail> Mistakes);
