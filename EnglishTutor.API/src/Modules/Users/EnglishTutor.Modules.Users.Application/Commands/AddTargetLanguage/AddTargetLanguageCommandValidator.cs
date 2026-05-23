using FluentValidation;

namespace EnglishTutor.Modules.Users.Application.Commands.AddTargetLanguage;

public sealed class AddTargetLanguageCommandValidator : AbstractValidator<AddTargetLanguageCommand>
{
    public AddTargetLanguageCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TargetLanguageCode).NotEmpty().MaximumLength(10);
        RuleFor(x => x.CurrentLevel).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TargetLevel).NotEmpty().MaximumLength(20);
    }
}
