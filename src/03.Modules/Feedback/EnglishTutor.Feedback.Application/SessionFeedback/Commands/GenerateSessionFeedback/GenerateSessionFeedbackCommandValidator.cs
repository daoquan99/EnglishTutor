using FluentValidation;

namespace EnglishTutor.Feedback.Application.SessionFeedback.Commands.GenerateSessionFeedback;

public sealed class GenerateSessionFeedbackCommandValidator : AbstractValidator<GenerateSessionFeedbackCommand>
{
    public GenerateSessionFeedbackCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("Session ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}
