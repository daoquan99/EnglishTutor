using EnglishTutor.BuildingBlocks.EventBus;

namespace EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;

public sealed record WrongAnswerDetail(
    Guid QuestionId,
    string Prompt,
    string UserAnswer,
    string CorrectAnswer,
    string? Explanation,
    string QuestionType);

public sealed record ExerciseCompletedIntegrationEvent(
    Guid UserId,
    Guid ExerciseSetId,
    Guid AttemptId,
    string TargetLanguageCode,
    string ExerciseType,
    int Score,
    int CorrectCount,
    int TotalQuestions,
    int TimeTakenSeconds,
    IReadOnlyCollection<WrongAnswerDetail> WrongAnswers,
    DateTime CompletedAtUtc) : IntegrationEvent;
