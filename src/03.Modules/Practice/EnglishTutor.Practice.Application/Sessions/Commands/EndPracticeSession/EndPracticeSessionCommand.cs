using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Practice.Contracts.Dtos;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Commands.EndPracticeSession;

public sealed record EndPracticeSessionCommand(
    Guid UserId,
    Guid SessionId) : ICommand<EndSessionResult>;

public sealed class EndPracticeSessionCommandValidator : AbstractValidator<EndPracticeSessionCommand>
{
    public EndPracticeSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
