using FluentValidation;
using System;

namespace EnglishTutor.Realtime.Application.Connections.Commands.LeavePracticeSession;

public sealed class LeavePracticeSessionCommandValidator : AbstractValidator<LeavePracticeSessionCommand>
{
    public LeavePracticeSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required.");
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("ConnectionId is required.");
    }
}
