using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Exercises.Application.Queries.GetExerciseById;

public sealed record GetExerciseByIdQuery(Guid ExerciseSetId) : IQuery<ExerciseDetailResponse>;
