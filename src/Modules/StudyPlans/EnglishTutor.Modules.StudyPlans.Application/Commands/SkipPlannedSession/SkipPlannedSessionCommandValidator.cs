using FluentValidation;

namespace EnglishTutor.Modules.StudyPlans.Application.Commands.SkipPlannedSession;

public sealed class SkipPlannedSessionCommandValidator : AbstractValidator<SkipPlannedSessionCommand>
{
    public SkipPlannedSessionCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.SessionId)
            .NotEmpty();
    }
}
