using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.Speaking.Domain.Events;

public sealed record SpeakingTurnCorrectedDomainEvent(
    Guid UserId,
    Guid SessionId,
    Guid TurnId,
    string TargetLanguageCode,
    string NativeLanguageCode,
    string ExplanationLanguageCode,
    string OriginalText,
    string CorrectedText,
    int GrammarScore,
    int VocabularyScore,
    int OverallScore,
    IReadOnlyList<SpeakingCorrectionMistake> Mistakes,
    DateTime CorrectedAtUtc) : DomainEvent;
