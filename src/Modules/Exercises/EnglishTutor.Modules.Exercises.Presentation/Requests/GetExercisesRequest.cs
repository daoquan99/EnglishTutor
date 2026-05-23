namespace EnglishTutor.Modules.Exercises.Presentation.Requests;

public sealed record GetExercisesRequest(
    int? Page,
    int? PageSize,
    string? Level,
    string? Type,
    string? Topic,
    string? Skill,
    string? TargetLanguageCode);
