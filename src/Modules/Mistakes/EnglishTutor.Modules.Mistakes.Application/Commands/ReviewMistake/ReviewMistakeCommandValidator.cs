using FluentValidation;

namespace EnglishTutor.Modules.Mistakes.Application.Commands.ReviewMistake;

public sealed class ReviewMistakeCommandValidator : AbstractValidator<ReviewMistakeCommand>
{
    public ReviewMistakeCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.MistakeId).NotEmpty();
    }
}
