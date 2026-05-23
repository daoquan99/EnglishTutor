using FluentValidation;

namespace EnglishTutor.Modules.Mistakes.Application.Commands.MarkMistakeMastered;

public sealed class MarkMistakeMasteredCommandValidator : AbstractValidator<MarkMistakeMasteredCommand>
{
    public MarkMistakeMasteredCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.MistakeId).NotEmpty();
    }
}
