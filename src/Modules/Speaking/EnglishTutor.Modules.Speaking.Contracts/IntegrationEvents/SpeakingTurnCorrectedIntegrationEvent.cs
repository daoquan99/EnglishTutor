using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.Speaking.Contracts.DTOs;

namespace EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;

public sealed record SpeakingTurnCorrectedIntegrationEvent(
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
    IReadOnlyList<SpeakingMistakeDetail> Mistakes,
    DateTime CorrectedAtUtc) : IntegrationEvent;
