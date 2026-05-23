using FluentValidation;

namespace EnglishTutor.Modules.Speaking.Application.Commands.StartSession;

public sealed class StartSpeakingSessionCommandValidator : AbstractValidator<StartSpeakingSessionCommand>
{
    public StartSpeakingSessionCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.SessionType)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(command => command.Topic)
            .MaximumLength(200)
            .When(command => command.Topic is not null);

        RuleFor(command => command.ConversationScenarioId!.Value)
            .NotEqual(Guid.Empty)
            .When(command => command.ConversationScenarioId.HasValue);
    }
}
