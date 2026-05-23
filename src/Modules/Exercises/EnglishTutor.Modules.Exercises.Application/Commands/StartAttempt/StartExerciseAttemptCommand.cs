using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Exercises.Application.Commands.StartAttempt;

public sealed record StartExerciseAttemptCommand(Guid UserId, Guid ExerciseSetId) : ICommand<StartExerciseAttemptResponse>;
