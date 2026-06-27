using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Commands.ExpirePracticeSession;

public sealed record ExpirePracticeSessionCommand(Guid SessionId) : ICommand;

public sealed class ExpirePracticeSessionCommandValidator : AbstractValidator<ExpirePracticeSessionCommand>
{
    public ExpirePracticeSessionCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
