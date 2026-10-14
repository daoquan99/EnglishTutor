using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Pronunciation;

public sealed class ScoringRubricEntity : AggregateRoot<Guid>
{
    public ScoringRubricEntity() { Id = Guid.NewGuid(); }

    public string Code { get; set; } = "ENGLISH_PRONUNCIATION_V1";
    public string Name { get; set; } = "English Pronunciation Rubric v1";
    public string Description { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public string AssessmentType { get; set; } = "Pronunciation";
    public bool IsActive { get; set; } = true;

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
