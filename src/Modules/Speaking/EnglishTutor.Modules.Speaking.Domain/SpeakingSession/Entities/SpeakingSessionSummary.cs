using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;

namespace EnglishTutor.Modules.Speaking.Domain.Entities;

public sealed class SpeakingSessionSummary : Entity<Guid>
{
    public Guid SpeakingSessionId { get; private set; }
    public Guid UserId { get; private set; }
    public LanguageCode TargetLanguageCode { get; private set; } = default!;
    public int AverageGrammarScore { get; private set; }
    public int AverageVocabularyScore { get; private set; }
    public int AveragePronunciationScore { get; private set; }
    public int AverageFluencyScore { get; private set; }
    public int OverallScore { get; private set; }
    public int TotalTurns { get; private set; }
    public int TotalMistakes { get; private set; }
    public string StrongPoints { get; private set; } = string.Empty;
    public string WeakPoints { get; private set; } = string.Empty;
    public string Recommendation { get; private set; } = string.Empty;

    private SpeakingSessionSummary() { }

    public static SpeakingSessionSummary Create(
        Guid speakingSessionId,
        Guid userId,
        LanguageCode targetLanguageCode,
        IReadOnlyCollection<SpeakingTurnResult> results,
        int totalMistakes,
        string strongPoints,
        string weakPoints,
        string recommendation)
    {
        if (speakingSessionId == Guid.Empty || userId == Guid.Empty)
        {
            throw new DomainException("Speaking session id and user id are required.");
        }

        if (results.Count == 0)
        {
            throw new DomainException("At least one speaking result is required to create a summary.");
        }

        return new SpeakingSessionSummary
        {
            Id = Guid.NewGuid(),
            SpeakingSessionId = speakingSessionId,
            UserId = userId,
            TargetLanguageCode = targetLanguageCode ?? throw new DomainException("Target language code is required."),
            AverageGrammarScore = (int)Math.Round(results.Average(result => result.GrammarScore)),
            AverageVocabularyScore = (int)Math.Round(results.Average(result => result.VocabularyScore)),
            AveragePronunciationScore = (int)Math.Round(results.Average(result => result.PronunciationScore)),
            AverageFluencyScore = (int)Math.Round(results.Average(result => result.FluencyScore)),
            OverallScore = (int)Math.Round(results.Average(result => result.OverallScore)),
            TotalTurns = results.Count,
            TotalMistakes = Math.Max(0, totalMistakes),
            StrongPoints = SpeakingTurn.NormalizeRequired(strongPoints, 2000, "Strong points"),
            WeakPoints = SpeakingTurn.NormalizeRequired(weakPoints, 2000, "Weak points"),
            Recommendation = SpeakingTurn.NormalizeRequired(recommendation, 2000, "Recommendation")
        };
    }
}
