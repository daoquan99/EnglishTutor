using FluentValidation;

namespace EnglishTutor.Modules.LearningContent.Application.Commands.CompleteConversation;

public sealed class CompleteConversationCommandValidator : AbstractValidator<CompleteConversationCommand>
{
    public CompleteConversationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ScenarioId).NotEmpty();
        RuleFor(x => x.DurationSeconds).GreaterThan(0);
    }
}
