using FluentValidation;

namespace EnglishTutor.Modules.Speaking.Application.Commands.CompleteSession;

public sealed class CompleteSpeakingSessionCommandValidator : AbstractValidator<CompleteSpeakingSessionCommand>
{
    public CompleteSpeakingSessionCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.SessionId)
            .NotEmpty();
    }
}
