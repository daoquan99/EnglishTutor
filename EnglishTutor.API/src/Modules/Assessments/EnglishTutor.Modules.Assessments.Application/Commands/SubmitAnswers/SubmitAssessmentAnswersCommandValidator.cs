using FluentValidation;

namespace EnglishTutor.Modules.Assessments.Application.Commands.SubmitAnswers;

public sealed class SubmitAssessmentAnswersCommandValidator : AbstractValidator<SubmitAssessmentAnswersCommand>
{
    public SubmitAssessmentAnswersCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.AttemptId).NotEmpty();
        RuleFor(command => command.Answers).NotEmpty();
        RuleForEach(command => command.Answers).SetValidator(new AnswerSubmissionValidator());
    }
}

internal sealed class AnswerSubmissionValidator : AbstractValidator<AnswerSubmission>
{
    public AnswerSubmissionValidator()
    {
        RuleFor(answer => answer.QuestionId).NotEmpty();
        RuleFor(answer => answer.UserAnswer).NotEmpty().MaximumLength(4000);
    }
}
