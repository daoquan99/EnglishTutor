namespace EnglishTutor.Modules.Speaking.Application.Shared.DTOs;

public sealed record SpeakingSessionSummaryResponse(
    Guid SessionId,
    int AverageGrammarScore,
    int AverageVocabularyScore,
    int AveragePronunciationScore,
    int AverageFluencyScore,
    int OverallScore,
    int TotalTurns,
    int TotalMistakes,
    string StrongPoints,
    string WeakPoints,
    string Recommendation);
