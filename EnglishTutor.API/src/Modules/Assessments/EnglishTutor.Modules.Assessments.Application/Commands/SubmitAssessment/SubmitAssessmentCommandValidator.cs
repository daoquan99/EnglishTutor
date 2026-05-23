using FluentValidation;

namespace EnglishTutor.Modules.Assessments.Application.Commands.SubmitAssessment;

public sealed class SubmitAssessmentCommandValidator : AbstractValidator<SubmitAssessmentCommand>
{
    public SubmitAssessmentCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.AttemptId).NotEmpty();
    }
}
