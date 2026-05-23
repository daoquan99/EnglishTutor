using FluentValidation;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.SubmitFillBlank;

public sealed class SubmitFillBlankCommandValidator : AbstractValidator<SubmitFillBlankCommand>
{
    public SubmitFillBlankCommandValidator()
    {
        RuleFor(command => command.TargetLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(command => command.UserAnswer).NotEmpty().MaximumLength(500);
    }
}
