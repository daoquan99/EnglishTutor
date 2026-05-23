using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Entities;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Events;

namespace EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt;

public sealed class UserExerciseAttempt : AggregateRoot<Guid>
{
    private readonly List<UserExerciseAnswer> _answers = [];

    public Guid UserId { get; private set; }
    public Guid ExerciseSetId { get; private set; }
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public DateTime StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public int TotalQuestions { get; private set; }
    public int CorrectCount { get; private set; }
    public int Score { get; private set; }
    public ExerciseAttemptStatus Status { get; private set; }
    public IReadOnlyCollection<UserExerciseAnswer> Answers => _answers.AsReadOnly();

    private UserExerciseAttempt() { }

    public static UserExerciseAttempt Start(
        Guid userId,
        Guid exerciseSetId,
        string targetLanguageCode,
        int totalQuestions,
        DateTime utcNow)
    {
        if (userId == Guid.Empty || exerciseSetId == Guid.Empty)
        {
            throw new DomainException("User id and exercise set id are required.");
        }

        if (totalQuestions <= 0)
        {
            throw new DomainException("Exercise attempt requires at least one question.");
        }

        var attempt = new UserExerciseAttempt
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ExerciseSetId = exerciseSetId,
            TargetLanguageCode = global::EnglishTutor.Modules.Exercises.Domain.ExerciseSet.ExerciseSet.NormalizeLanguage(targetLanguageCode),
            StartedAtUtc = utcNow,
            TotalQuestions = totalQuestions,
            Status = ExerciseAttemptStatus.InProgress,
            CreatedAtUtc = utcNow
        };

        attempt.AddDomainEvent(new ExerciseStartedDomainEvent(
            userId,
            attempt.Id,
            exerciseSetId,
            attempt.TargetLanguageCode,
            utcNow));

        return attempt;
    }

    public bool HasAnswered(Guid questionId) =>
        _answers.Any(answer => answer.QuestionId == questionId);

    public UserExerciseAnswer RecordAnswer(
        Guid questionId,
        string userAnswer,
        bool isCorrect,
        int score,
        string? feedback,
        DateTime utcNow)
    {
        EnsureInProgress();

        if (questionId == Guid.Empty)
        {
            throw new DomainException("Question id is required.");
        }

        if (HasAnswered(questionId))
        {
            throw new DomainException("Question has already been answered.");
        }

        var answer = UserExerciseAnswer.Create(Id, questionId, userAnswer, isCorrect, score, feedback, utcNow);
        _answers.Add(answer);

        if (answer.IsCorrect)
        {
            CorrectCount++;
        }

        AddDomainEvent(new ExerciseQuestionAnsweredDomainEvent(
            UserId,
            Id,
            ExerciseSetId,
            questionId,
            TargetLanguageCode,
            answer.IsCorrect,
            answer.Score,
            answer.AnsweredAtUtc));

        return answer;
    }

    public void Complete(DateTime utcNow, string exerciseType = "", IReadOnlyCollection<ExerciseCompletedWrongAnswer>? wrongAnswers = null)
    {
        EnsureInProgress();

        if (_answers.Count != TotalQuestions)
        {
            throw new DomainException("All questions must be answered before completing an exercise.");
        }

        CompletedAtUtc = utcNow;
        Score = TotalQuestions == 0 ? 0 : (int)Math.Round(CorrectCount * 100m / TotalQuestions);
        Status = ExerciseAttemptStatus.Completed;
        UpdatedAtUtc = utcNow;

        AddDomainEvent(new ExerciseCompletedDomainEvent(
            UserId,
            ExerciseSetId,
            Id,
            TargetLanguageCode,
            exerciseType,
            Score,
            CorrectCount,
            TotalQuestions,
            Math.Max(0, (int)(utcNow - StartedAtUtc).TotalSeconds),
            wrongAnswers?.ToArray() ?? [],
            utcNow));
    }

    private void EnsureInProgress()
    {
        if (Status != ExerciseAttemptStatus.InProgress)
        {
            throw new DomainException("Exercise attempt is already completed.");
        }
    }
}
