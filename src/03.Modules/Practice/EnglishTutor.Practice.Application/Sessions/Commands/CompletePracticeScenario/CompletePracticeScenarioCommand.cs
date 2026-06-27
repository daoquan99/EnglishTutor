using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Practice.Contracts.Dtos;
using FluentValidation;

namespace EnglishTutor.Practice.Application.Sessions.Commands.CompletePracticeScenario;

public sealed record CompletePracticeScenarioCommand(
    Guid UserId,
    Guid SessionId) : ICommand<EndSessionResult>;

public sealed class CompletePracticeScenarioCommandValidator : AbstractValidator<CompletePracticeScenarioCommand>
{
    public CompletePracticeScenarioCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
    }
}
