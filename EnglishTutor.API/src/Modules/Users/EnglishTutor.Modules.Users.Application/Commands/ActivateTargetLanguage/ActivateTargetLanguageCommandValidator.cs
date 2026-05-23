using FluentValidation;

namespace EnglishTutor.Modules.Users.Application.Commands.ActivateTargetLanguage;

public sealed class ActivateTargetLanguageCommandValidator : AbstractValidator<ActivateTargetLanguageCommand>
{
    public ActivateTargetLanguageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TargetLanguageId).NotEmpty();
    }
}
