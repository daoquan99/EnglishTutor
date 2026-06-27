using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.Scenarios.UpdateScenario;

public sealed record UpdateScenarioCommand(
    Guid ScenarioId,
    string Name,
    string? Description,
    string DifficultyLevel,
    string PromptTemplate,
    Guid? CurrentUserId) : ICommand;

public sealed class UpdateScenarioCommandValidator : AbstractValidator<UpdateScenarioCommand>
{
    public UpdateScenarioCommandValidator()
    {
        RuleFor(c => c.ScenarioId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(255);
        RuleFor(c => c.DifficultyLevel).NotEmpty().MaximumLength(50);
        RuleFor(c => c.PromptTemplate).NotEmpty();
    }
}
