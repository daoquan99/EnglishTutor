using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;

public sealed class AssessmentGradingResult : Entity<Guid>
{
    public Guid AttemptId { get; private set; }
    public int TotalScore { get; private set; }
    public string SectionScoresJson { get; private set; } = string.Empty;
    public bool IsPassed { get; private set; }
    public DateTime GradedAtUtc { get; private set; }
    public string? GradingNotes { get; private set; }

    private AssessmentGradingResult() { }

    public static AssessmentGradingResult Create(Guid attemptId, int totalScore, string sectionScoresJson, bool isPassed, DateTime gradedAtUtc, string? gradingNotes)
    {
        if (attemptId == Guid.Empty)
        {
            throw new DomainException("Assessment attempt id is required.");
        }

        return new AssessmentGradingResult
        {
            Id = Guid.NewGuid(),
            AttemptId = attemptId,
            TotalScore = Math.Clamp(totalScore, 0, 100),
            SectionScoresJson = AssessmentDefinition.AssessmentDefinition.NormalizeRequired(sectionScoresJson, 8000, "Section scores json"),
            IsPassed = isPassed,
            GradedAtUtc = gradedAtUtc,
            GradingNotes = AssessmentDefinition.AssessmentDefinition.NormalizeOptional(gradingNotes, 4000, "Grading notes"),
            CreatedAtUtc = gradedAtUtc
        };
    }
}
