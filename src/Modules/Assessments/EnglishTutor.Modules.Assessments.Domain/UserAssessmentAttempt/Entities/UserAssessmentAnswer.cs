using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;

public sealed class UserAssessmentAnswer : Entity<Guid>
{
    public Guid AttemptId { get; private set; }
    public Guid QuestionId { get; private set; }
    public string UserAnswer { get; private set; } = string.Empty;
    public int? Score { get; private set; }
    public string? Feedback { get; private set; }
    public bool IsGraded { get; private set; }
    public DateTime AnsweredAtUtc { get; private set; }

    private UserAssessmentAnswer() { }

    internal static UserAssessmentAnswer Create(Guid attemptId, Guid questionId, string userAnswer, DateTime utcNow)
    {
        if (attemptId == Guid.Empty || questionId == Guid.Empty)
        {
            throw new DomainException("Attempt id and question id are required.");
        }

        return new UserAssessmentAnswer
        {
            Id = Guid.NewGuid(),
            AttemptId = attemptId,
            QuestionId = questionId,
            UserAnswer = AssessmentDefinition.AssessmentDefinition.NormalizeRequired(userAnswer, 4000, "User answer"),
            AnsweredAtUtc = utcNow,
            CreatedAtUtc = utcNow
        };
    }

    public void ApplyScore(int score, string? feedback, DateTime utcNow)
    {
        Score = Math.Clamp(score, 0, 100);
        Feedback = AssessmentDefinition.AssessmentDefinition.NormalizeOptional(feedback, 4000, "Feedback");
        IsGraded = true;
        UpdatedAtUtc = utcNow;
    }
}
