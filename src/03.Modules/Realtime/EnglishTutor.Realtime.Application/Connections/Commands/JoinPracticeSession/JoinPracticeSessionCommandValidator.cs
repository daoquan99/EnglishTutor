using FluentValidation;
using System;

namespace EnglishTutor.Realtime.Application.Connections.Commands.JoinPracticeSession;

public sealed class JoinPracticeSessionCommandValidator : AbstractValidator<JoinPracticeSessionCommand>
{
    public JoinPracticeSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required.");
        RuleFor(x => x.SessionId).NotEmpty().WithMessage("SessionId is required.");
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("ConnectionId is required.");
    }
}
