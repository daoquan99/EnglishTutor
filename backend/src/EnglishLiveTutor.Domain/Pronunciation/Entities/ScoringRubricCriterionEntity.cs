using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Pronunciation;

public sealed class ScoringRubricCriterionEntity : Entity<Guid>
{
    public ScoringRubricCriterionEntity() { Id = Guid.NewGuid(); }

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
