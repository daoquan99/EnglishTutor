using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Exercises.Domain.Shared;

namespace EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Entities;

public sealed class UserExerciseResult : Entity<Guid>
{
    public Guid AttemptId { get; private set; }
    public Guid UserId { get; private set; }
    public Guid ExerciseSetId { get; private set; }
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public ExerciseType ExerciseType { get; private set; }
    public int TotalScore { get; private set; }
    public int CorrectCount { get; private set; }
    public int TotalQuestions { get; private set; }
    public int TimeTakenSeconds { get; private set; }
    public DateTime CompletedAtUtc { get; private set; }

    private UserExerciseResult() { }

    public static UserExerciseResult Create(
        Guid attemptId,
        Guid userId,
        Guid exerciseSetId,
        string targetLanguageCode,
        ExerciseType exerciseType,
        int totalScore,
        int correctCount,
        int totalQuestions,
        int timeTakenSeconds,
        DateTime completedAtUtc)
    {
        if (attemptId == Guid.Empty || userId == Guid.Empty || exerciseSetId == Guid.Empty)
        {
            throw new DomainException("Attempt, user, and exercise set ids are required.");
        }

        return new UserExerciseResult
        {
            Id = Guid.NewGuid(),
            AttemptId = attemptId,
            UserId = userId,
            ExerciseSetId = exerciseSetId,
            TargetLanguageCode = global::EnglishTutor.Modules.Exercises.Domain.ExerciseSet.ExerciseSet.NormalizeLanguage(targetLanguageCode),
            ExerciseType = exerciseType,
            TotalScore = Math.Clamp(totalScore, 0, 100),
            CorrectCount = Math.Max(0, correctCount),
            TotalQuestions = Math.Max(0, totalQuestions),
            TimeTakenSeconds = Math.Max(0, timeTakenSeconds),
            CompletedAtUtc = completedAtUtc,
            CreatedAtUtc = completedAtUtc
        };
    }
}
