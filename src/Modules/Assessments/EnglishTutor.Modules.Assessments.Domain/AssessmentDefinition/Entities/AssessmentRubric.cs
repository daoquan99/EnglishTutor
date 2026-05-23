using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Assessments.Domain.Shared;

namespace EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;

public sealed class AssessmentRubric : Entity<Guid>
{
    public Guid AssessmentDefinitionId { get; private set; }
    public AssessmentSkill Skill { get; private set; }
    public string Criteria { get; private set; } = string.Empty;
    public int MaxScore { get; private set; }
    public string ScoringGuide { get; private set; } = string.Empty;

    private AssessmentRubric() { }

    internal static AssessmentRubric Create(Guid assessmentDefinitionId, AssessmentSkill skill, string criteria, int maxScore, string scoringGuide, DateTime utcNow)
    {
        if (assessmentDefinitionId == Guid.Empty)
        {
            throw new DomainException("Assessment definition id is required.");
        }

        return new AssessmentRubric
        {
            Id = Guid.NewGuid(),
            AssessmentDefinitionId = assessmentDefinitionId,
            Skill = skill,
            Criteria = AssessmentDefinition.NormalizeRequired(criteria, 1000, "Rubric criteria"),
            MaxScore = Math.Clamp(maxScore, 1, 100),
            ScoringGuide = AssessmentDefinition.NormalizeRequired(scoringGuide, 4000, "Scoring guide"),
            CreatedAtUtc = utcNow
        };
    }
}
