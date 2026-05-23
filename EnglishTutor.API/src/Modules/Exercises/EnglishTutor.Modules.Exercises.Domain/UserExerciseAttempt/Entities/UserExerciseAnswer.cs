using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Entities;

public sealed class UserExerciseAnswer : Entity<Guid>
{
    public Guid AttemptId { get; private set; }
    public Guid QuestionId { get; private set; }
    public string UserAnswer { get; private set; } = string.Empty;
    public bool IsCorrect { get; private set; }
    public int Score { get; private set; }
    public string? Feedback { get; private set; }
    public DateTime AnsweredAtUtc { get; private set; }

    private UserExerciseAnswer() { }

    internal static UserExerciseAnswer Create(
        Guid attemptId,
        Guid questionId,
        string userAnswer,
        bool isCorrect,
        int score,
        string? feedback,
        DateTime utcNow)
    {
        if (attemptId == Guid.Empty || questionId == Guid.Empty)
        {
            throw new DomainException("Attempt id and question id are required.");
        }

        return new UserExerciseAnswer
        {
            Id = Guid.NewGuid(),
            AttemptId = attemptId,
            QuestionId = questionId,
            UserAnswer = global::EnglishTutor.Modules.Exercises.Domain.ExerciseSet.ExerciseSet.NormalizeRequired(userAnswer, 4000, "User answer"),
            IsCorrect = isCorrect,
            Score = Math.Clamp(score, 0, 100),
            Feedback = global::EnglishTutor.Modules.Exercises.Domain.ExerciseSet.ExerciseSet.NormalizeOptional(feedback, 4000, "Feedback"),
            AnsweredAtUtc = utcNow,
            CreatedAtUtc = utcNow
        };
    }
}
