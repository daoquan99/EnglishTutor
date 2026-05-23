using FluentValidation;

namespace EnglishTutor.Modules.Vocabulary.Application.Commands.UpdateStudySettings;

public sealed class UpdateStudySettingsCommandValidator : AbstractValidator<UpdateStudySettingsCommand>
{
    public UpdateStudySettingsCommandValidator()
    {
        RuleFor(command => command.TargetLanguageCode).NotEmpty().Length(2, 3);
        RuleFor(command => command.NewWordsPerDay).InclusiveBetween(1, 50);
        RuleFor(command => command.ReviewWordsPerDay).InclusiveBetween(1, 100);
    }
}
