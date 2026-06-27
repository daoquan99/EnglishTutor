using FluentValidation;
using System;

namespace EnglishTutor.Identity.Application.Commands.RevokeSession;

public sealed class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
{
    public RevokeSessionCommandValidator()
    {
        RuleFor(c => c.SessionId).NotEmpty();
        RuleFor(c => c.UserId).NotEmpty();
    }
}
