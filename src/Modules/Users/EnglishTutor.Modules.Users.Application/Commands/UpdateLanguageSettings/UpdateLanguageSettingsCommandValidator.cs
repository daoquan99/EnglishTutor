using FluentValidation;

namespace EnglishTutor.Modules.Users.Application.Commands.UpdateLanguageSettings;

public sealed class UpdateLanguageSettingsCommandValidator : AbstractValidator<UpdateLanguageSettingsCommand>
{
    public UpdateLanguageSettingsCommandValidator()
    {
        RuleFor(command => command.NativeLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(command => command.UiLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(command => command.ExplanationLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(command => command.ActiveTargetLanguageCode).NotEmpty().Length(2, 3);
    }
}
