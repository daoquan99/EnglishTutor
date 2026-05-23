using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Assessments.Domain.Shared;

namespace EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;

public sealed class AssessmentSection : Entity<Guid>
{
    private readonly List<AssessmentQuestion> _questions = [];

    public Guid AssessmentDefinitionId { get; private set; }
    public AssessmentSkill Skill { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Weight { get; private set; }
    public int Order { get; private set; }
    public IReadOnlyCollection<AssessmentQuestion> Questions => _questions.AsReadOnly();

    private AssessmentSection() { }

    internal static AssessmentSection Create(Guid assessmentDefinitionId, AssessmentSkill skill, string title, decimal weight, int order, DateTime utcNow)
    {
        if (assessmentDefinitionId == Guid.Empty)
        {
            throw new DomainException("Assessment definition id is required.");
        }

        if (weight <= 0m || weight > 1m)
        {
            throw new DomainException("Assessment section weight must be between 0 and 1.");
        }

        return new AssessmentSection
        {
            Id = Guid.NewGuid(),
            AssessmentDefinitionId = assessmentDefinitionId,
            Skill = skill,
            Title = AssessmentDefinition.NormalizeRequired(title, 200, "Section title"),
            Weight = weight,
            Order = order,
            CreatedAtUtc = utcNow
        };
    }

    public AssessmentQuestion AddQuestion(
        string prompt,
        string questionType,
        string? correctAnswer,
        bool isAiGraded,
        int maxScore,
        int order,
        string? explanation,
        DateTime utcNow)
    {
        if (_questions.Any(question => question.Order == order))
        {
            throw new DomainException("Assessment question order must be unique within a section.");
        }

        if (!isAiGraded && string.IsNullOrWhiteSpace(correctAnswer))
        {
            throw new DomainException("Static assessment questions require a correct answer.");
        }

        var question = AssessmentQuestion.Create(Id, prompt, questionType, correctAnswer, isAiGraded, maxScore, order, explanation, utcNow);
        _questions.Add(question);
        return question;
    }
}
