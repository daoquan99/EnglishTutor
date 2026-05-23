using FluentValidation;

namespace EnglishTutor.Modules.Assessments.Application.Commands.StartLevelUpAssessment;

public sealed class StartLevelUpAssessmentCommandValidator : AbstractValidator<StartLevelUpAssessmentCommand>
{
    public StartLevelUpAssessmentCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.TargetLanguageCode).MaximumLength(10).When(command => command.TargetLanguageCode is not null);
    }
}
