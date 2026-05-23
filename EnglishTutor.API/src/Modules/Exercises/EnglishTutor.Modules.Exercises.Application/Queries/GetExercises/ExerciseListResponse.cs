namespace EnglishTutor.Modules.Exercises.Application.Queries.GetExercises;

public sealed record ExerciseListResponse(
    Guid Id,
    string TargetLanguageCode,
    string Level,
    string Topic,
    string Skill,
    string ExerciseType,
    string Title,
    string? Description,
    int TotalQuestions);
