namespace EnglishTutor.Modules.Speaking.Application.Commands.AddTurn;

public sealed record SpeakingTurnResponse(
    Guid TurnId,
    string OriginalText,
    string CorrectedText,
    string NaturalVersion,
    int GrammarScore,
    int VocabularyScore,
    int OverallScore,
    string Feedback);
