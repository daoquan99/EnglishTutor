using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.Scenarios.DisableScenario;

public sealed record DisableScenarioCommand(
    Guid ScenarioId,
    Guid? CurrentUserId) : ICommand;

public sealed class DisableScenarioCommandValidator : AbstractValidator<DisableScenarioCommand>
{
    public DisableScenarioCommandValidator()
    {
        RuleFor(c => c.ScenarioId).NotEmpty();
    }
}
