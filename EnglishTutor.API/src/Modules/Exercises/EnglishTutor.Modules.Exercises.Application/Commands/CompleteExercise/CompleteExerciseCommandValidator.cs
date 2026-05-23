using FluentValidation;

namespace EnglishTutor.Modules.Exercises.Application.Commands.CompleteExercise;

public sealed class CompleteExerciseCommandValidator : AbstractValidator<CompleteExerciseCommand>
{
    public CompleteExerciseCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.AttemptId).NotEmpty();
    }
}
