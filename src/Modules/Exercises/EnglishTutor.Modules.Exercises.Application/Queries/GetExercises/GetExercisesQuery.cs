using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Exercises.Application.Queries.GetExercises;

public sealed record GetExercisesQuery(
    Guid UserId,
    int Page,
    int PageSize,
    string? Level,
    string? Type,
    string? Topic,
    string? Skill,
    string? TargetLanguageCode) : IQuery<IReadOnlyList<ExerciseListResponse>>;
