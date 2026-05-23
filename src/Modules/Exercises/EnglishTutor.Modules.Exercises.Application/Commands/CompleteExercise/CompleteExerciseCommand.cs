using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Exercises.Application.Commands.CompleteExercise;

public sealed record CompleteExerciseCommand(Guid UserId, Guid AttemptId) : ICommand<ExerciseResultResponse>;
