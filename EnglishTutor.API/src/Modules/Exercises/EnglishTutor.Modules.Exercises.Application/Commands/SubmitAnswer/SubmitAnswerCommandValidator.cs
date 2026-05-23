using FluentValidation;

namespace EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;

public sealed class SubmitAnswerCommandValidator : AbstractValidator<SubmitAnswerCommand>
{
    public SubmitAnswerCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.AttemptId).NotEmpty();
        RuleFor(command => command.QuestionId).NotEmpty();
        RuleFor(command => command.UserAnswer).NotEmpty().MaximumLength(4000);
    }
}
