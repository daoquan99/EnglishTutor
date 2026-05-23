using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Exercises.Application.Queries.GetAttemptResult;

public sealed record GetAttemptResultQuery(Guid UserId, Guid AttemptId) : IQuery<ExerciseResultResponse>;
