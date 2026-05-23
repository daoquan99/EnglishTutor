namespace EnglishTutor.Modules.Exercises.Application.Commands.StartAttempt;

public sealed record StartExerciseAttemptResponse(
    Guid AttemptId,
    Guid ExerciseSetId,
    string TargetLanguageCode,
    int TotalQuestions,
    DateTime StartedAtUtc);
