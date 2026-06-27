using FluentValidation;
using System;

namespace EnglishTutor.Realtime.Application.Connections.Commands.RegisterHeartbeat;

public sealed class RegisterHeartbeatCommandValidator : AbstractValidator<RegisterHeartbeatCommand>
{
    public RegisterHeartbeatCommandValidator()
    {
        RuleFor(x => x.ConnectionId).NotEmpty().WithMessage("ConnectionId is required.");
    }
}
