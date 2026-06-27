using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Practice.Contracts.Dtos;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Commands.CancelPracticeSession;

public sealed record CancelPracticeSessionCommand(
    Guid UserId,
    Guid SessionId) : ICommand<EndSessionResult>;

public sealed class CancelPracticeSessionCommandValidator : AbstractValidator<CancelPracticeSessionCommand>
{
    public CancelPracticeSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
