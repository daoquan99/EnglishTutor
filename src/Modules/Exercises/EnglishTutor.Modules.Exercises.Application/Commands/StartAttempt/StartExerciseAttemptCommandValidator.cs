using FluentValidation;

namespace EnglishTutor.Modules.Exercises.Application.Commands.StartAttempt;

public sealed class StartExerciseAttemptCommandValidator : AbstractValidator<StartExerciseAttemptCommand>
{
    public StartExerciseAttemptCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.ExerciseSetId).NotEmpty();
    }
}
