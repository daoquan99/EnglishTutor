using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Pronunciation;

public sealed class PronunciationAssessmentEntity : AggregateRoot<Guid>
{
    public PronunciationAssessmentEntity() { Id = Guid.NewGuid(); }

    public string UserId { get; set; } = "default-learner";
    public Guid VocabularyId { get; set; }
    public Guid SenseId { get; set; }
    public string TargetType { get; set; } = "Word"; // Word | Sentence
    public Guid? TargetId { get; set; }
    public string ReferenceText { get; set; } = string.Empty;
    public string Accent { get; set; } = "US";
    public string ModelCode { get; set; } = "gemini-3.8-flash";
    public Guid RubricId { get; set; }
    public int RubricVersion { get; set; } = 1;
    public Guid AudioRecordingId { get; set; }
    public string? RecognizedText { get; set; }
    public decimal? OverallScore { get; set; }
    public decimal? Confidence { get; set; }
    public string Status { get; set; } = "Completed"; // Pending | Processing | Completed | NotAssessable | Failed
    public string? Feedback { get; set; }
    public string? StrengthsJson { get; set; }
    public string? ImprovementsJson { get; set; }
    public string? SuggestedPracticeJson { get; set; }
    public string? ErrorCode { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public List<PronunciationCriterionScoreEntity> CriterionScores { get; set; } = new();

    public static decimal CalculateWeightedScore(decimal rawScore, decimal weight)
    {
        return Math.Round(rawScore * weight / 100.00m, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal? CalculateOverallScore(IEnumerable<decimal> weightedScores, bool isAssessable)
    {
        return isAssessable
            ? Math.Round(weightedScores.Sum(), 2, MidpointRounding.AwayFromZero)
            : null;
    }
}
