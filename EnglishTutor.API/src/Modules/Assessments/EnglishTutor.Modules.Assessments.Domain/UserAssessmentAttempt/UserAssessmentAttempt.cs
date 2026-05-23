using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Assessments.Domain.Shared;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Enums;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Events;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Rules;

namespace EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;

public sealed class UserAssessmentAttempt : AggregateRoot<Guid>
{
    private readonly List<Entities.UserAssessmentAnswer> _answers = [];

    public Guid UserId { get; private set; }
    public Guid AssessmentDefinitionId { get; private set; }
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public string CurrentLevel { get; private set; } = string.Empty;
    public int? TotalScore { get; private set; }
    public AssessmentAttemptStatus Status { get; private set; }
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? SubmittedAtUtc { get; private set; }
    public DateTime? GradedAtUtc { get; private set; }
    public IReadOnlyCollection<Entities.UserAssessmentAnswer> Answers => _answers.AsReadOnly();

    private UserAssessmentAttempt() { }

    public static UserAssessmentAttempt Start(Guid userId, Guid assessmentDefinitionId, string targetLanguageCode, string currentLevel, DateTime utcNow)
    {
        if (userId == Guid.Empty || assessmentDefinitionId == Guid.Empty)
        {
            throw new DomainException("User id and assessment definition id are required.");
        }

        var attempt = new UserAssessmentAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AssessmentDefinitionId = assessmentDefinitionId,
            TargetLanguageCode = AssessmentDefinition.AssessmentDefinition.NormalizeLanguage(targetLanguageCode),
            CurrentLevel = AssessmentDefinition.AssessmentDefinition.NormalizeRequired(currentLevel, 10, "Current level"),
            Status = AssessmentAttemptStatus.InProgress,
            StartedAtUtc = utcNow,
            CreatedAtUtc = utcNow
        };

        attempt.AddDomainEvent(new AssessmentStartedDomainEvent(userId, attempt.Id, assessmentDefinitionId, attempt.TargetLanguageCode, attempt.CurrentLevel, utcNow));
        return attempt;
    }

    public void SubmitAnswer(Guid questionId, string userAnswer, DateTime utcNow)
    {
        EnsureInProgress();
        if (_answers.Any(answer => answer.QuestionId == questionId))
        {
            throw new DomainException("Assessment question has already been answered.");
        }

        _answers.Add(Entities.UserAssessmentAnswer.Create(Id, questionId, userAnswer, utcNow));
        UpdatedAtUtc = utcNow;
    }

    public void Submit(int expectedAnswerCount, DateTime utcNow)
    {
        EnsureInProgress();
        if (_answers.Count != expectedAnswerCount)
        {
            throw new DomainException("All assessment questions must be answered before submission.");
        }

        Status = AssessmentAttemptStatus.Submitted;
        SubmittedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void MarkGrading(DateTime utcNow)
    {
        if (Status != AssessmentAttemptStatus.Submitted)
        {
            throw new DomainException("Assessment attempt must be submitted before grading.");
        }

        Status = AssessmentAttemptStatus.Grading;
        UpdatedAtUtc = utcNow;
    }

    public void ApplyGradingResult(IReadOnlyDictionary<AssessmentSkill, int> sectionScores, int totalScore, int passingScore, int minSkillScore, DateTime utcNow)
    {
        if (Status != AssessmentAttemptStatus.Grading)
        {
            throw new DomainException("Assessment attempt must be grading before applying results.");
        }

        TotalScore = Math.Clamp(totalScore, 0, 100);
        GradedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;

        var passRule = new AssessmentPassRule(TotalScore.Value, sectionScores, passingScore, minSkillScore);
        if (passRule.IsBroken())
        {
            Status = AssessmentAttemptStatus.Failed;
            AddDomainEvent(new AssessmentFailedDomainEvent(UserId, Id, AssessmentDefinitionId, TargetLanguageCode, CurrentLevel, TotalScore.Value, sectionScores, utcNow));
            return;
        }

        Status = AssessmentAttemptStatus.Passed;
        AddDomainEvent(new AssessmentPassedDomainEvent(UserId, Id, AssessmentDefinitionId, TargetLanguageCode, CurrentLevel, TotalScore.Value, sectionScores, utcNow));
    }

    private void EnsureInProgress()
    {
        if (Status != AssessmentAttemptStatus.InProgress)
        {
            throw new DomainException("Assessment attempt is not in progress.");
        }
    }
}
