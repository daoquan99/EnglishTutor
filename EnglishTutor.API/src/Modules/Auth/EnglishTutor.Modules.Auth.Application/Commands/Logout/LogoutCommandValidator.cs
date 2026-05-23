using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.DeviceId)
            .NotEmpty();
    }
}
