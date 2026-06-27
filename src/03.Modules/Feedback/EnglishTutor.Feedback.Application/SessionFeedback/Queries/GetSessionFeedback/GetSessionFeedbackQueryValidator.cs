using FluentValidation;

namespace EnglishTutor.Feedback.Application.SessionFeedback.Queries.GetSessionFeedback;

public sealed class GetSessionFeedbackQueryValidator : AbstractValidator<GetSessionFeedbackQuery>
{
    public GetSessionFeedbackQueryValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("Session ID is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}
