using FluentValidation;

namespace EnglishTutor.Modules.Auth.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty();

        RuleFor(x => x.SessionId)
            .NotEmpty();

        RuleFor(x => x.DeviceId)
            .NotEmpty();
    }
}
