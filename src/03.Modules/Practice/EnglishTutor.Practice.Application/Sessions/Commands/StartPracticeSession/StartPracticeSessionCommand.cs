using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Practice.Contracts.Dtos;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Commands.StartPracticeSession;

public sealed record StartPracticeSessionCommand(
    Guid UserId,
    Guid ScenarioId,
    string IdempotencyKey,
    int RequestedMinutes) : ICommand<StartSessionResult>;

public sealed class StartPracticeSessionCommandValidator : AbstractValidator<StartPracticeSessionCommand>
{
    public StartPracticeSessionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.ScenarioId).NotEmpty();
        RuleFor(x => x.IdempotencyKey).NotEmpty();
        RuleFor(x => x.RequestedMinutes).GreaterThan(0);
    }
}
