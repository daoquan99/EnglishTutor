namespace EnglishLiveTutor.Domain.Pronunciation;

public sealed class ScoringRubricEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = "ENGLISH_PRONUNCIATION_V1";
    public string Name { get; set; } = "English Pronunciation Rubric v1";
    public string Description { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public string AssessmentType { get; set; } = "Pronunciation";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<ScoringRubricCriterionEntity> Criteria { get; set; } = new();

    public void ValidateWeights()
    {
        if (Criteria is null || Criteria.Count == 0)
            throw new InvalidOperationException("Rubric phải có ít nhất 1 tiêu chí chấm điểm.");

        decimal totalWeight = Criteria.Sum(c => c.Weight);
        if (Math.Abs(totalWeight - 100.00m) > 0.01m)
            throw new InvalidOperationException($"Tổng trọng số các tiêu chí phải bằng 100% (hiện tại: {totalWeight}%).");
    }
}

public sealed class ScoringRubricCriterionEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RubricId { get; set; }
    public string Code { get; set; } = string.Empty; // Accuracy | Clarity | Fluency
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Weight { get; set; } // 40.00 | 30.00 | 30.00
    public decimal MinimumScore { get; set; } = 0m;
    public decimal MaximumScore { get; set; } = 100m;
    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}

public sealed class AudioRecordingEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = "default-learner";
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = "audio/webm";
    public long FileSizeBytes { get; set; }
    public int DurationMilliseconds { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }
    public string DeletionStatus { get; set; } = "Retained";
}

public sealed class PronunciationAssessmentEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
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
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
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

public sealed class PronunciationCriterionScoreEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AssessmentId { get; set; }
    public string CriterionCode { get; set; } = string.Empty;
    public string CriterionName { get; set; } = string.Empty;
    public string CriterionDescriptionSnapshot { get; set; } = string.Empty;
    public decimal WeightSnapshot { get; set; }
    public decimal RawScore { get; set; }
    public decimal WeightedScore { get; set; }
    public string Feedback { get; set; } = string.Empty;
}
