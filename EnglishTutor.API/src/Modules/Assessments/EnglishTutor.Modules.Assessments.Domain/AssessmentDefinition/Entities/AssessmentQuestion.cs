using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;

public sealed class AssessmentQuestion : Entity<Guid>
{
    public Guid SectionId { get; private set; }
    public string Prompt { get; private set; } = string.Empty;
    public string QuestionType { get; private set; } = string.Empty;
    public string? CorrectAnswer { get; private set; }
    public bool IsAiGraded { get; private set; }
    public int MaxScore { get; private set; }
    public int Order { get; private set; }
    public string? Explanation { get; private set; }

    private AssessmentQuestion() { }

    internal static AssessmentQuestion Create(
        Guid sectionId,
        string prompt,
        string questionType,
        string? correctAnswer,
        bool isAiGraded,
        int maxScore,
        int order,
        string? explanation,
        DateTime utcNow)
    {
        if (sectionId == Guid.Empty)
        {
            throw new DomainException("Assessment section id is required.");
        }

        if (maxScore <= 0 || maxScore > 100)
        {
            throw new DomainException("Assessment question max score must be between 1 and 100.");
        }

        return new AssessmentQuestion
        {
            Id = Guid.NewGuid(),
            SectionId = sectionId,
            Prompt = AssessmentDefinition.NormalizeRequired(prompt, 4000, "Question prompt"),
            QuestionType = AssessmentDefinition.NormalizeRequired(questionType, 100, "Question type"),
            CorrectAnswer = AssessmentDefinition.NormalizeOptional(correctAnswer, 4000, "Correct answer"),
            IsAiGraded = isAiGraded,
            MaxScore = maxScore,
            Order = order,
            Explanation = AssessmentDefinition.NormalizeOptional(explanation, 4000, "Explanation"),
            CreatedAtUtc = utcNow
        };
    }
}
