using FluentValidation;

namespace EnglishTutor.Modules.AI.Application.Commands.CorrectSentence;

public sealed class CorrectSentenceCommandValidator : AbstractValidator<CorrectSentenceCommand>
{
    public CorrectSentenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.OriginalText)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.LanguageContext)
            .NotNull();
    }
}
