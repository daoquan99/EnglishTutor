using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Pronunciation;

public sealed class PronunciationCriterionScoreEntity : Entity<Guid>
{
    public PronunciationCriterionScoreEntity() { Id = Guid.NewGuid(); }

    public Guid AssessmentId { get; set; }
    public string CriterionCode { get; set; } = string.Empty;
    public string CriterionName { get; set; } = string.Empty;
    public string CriterionDescriptionSnapshot { get; set; } = string.Empty;
    public decimal WeightSnapshot { get; set; }
    public decimal RawScore { get; set; }
    public decimal WeightedScore { get; set; }
    public string Feedback { get; set; } = string.Empty;
}
