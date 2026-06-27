using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.Scenarios.CreateScenario;

public sealed record CreateScenarioCommand(
    Guid TopicId,
    Guid ModeDefinitionId,
    string Name,
    string? Description,
    string DifficultyLevel,
    string PromptTemplate,
    Guid? CurrentUserId) : ICommand<Guid>;

public sealed class CreateScenarioCommandValidator : AbstractValidator<CreateScenarioCommand>
{
    public CreateScenarioCommandValidator()
    {
        RuleFor(c => c.TopicId).NotEmpty();
        RuleFor(c => c.ModeDefinitionId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(255);
        RuleFor(c => c.DifficultyLevel).NotEmpty().MaximumLength(50);
        RuleFor(c => c.PromptTemplate).NotEmpty();
    }
}
