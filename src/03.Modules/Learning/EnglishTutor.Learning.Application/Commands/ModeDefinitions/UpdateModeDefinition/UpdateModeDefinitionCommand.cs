using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.ModeDefinitions.UpdateModeDefinition;

public sealed record UpdateModeDefinitionCommand(
    Guid ModeDefinitionId,
    string Name,
    string? Description,
    Guid? CurrentUserId) : ICommand;

public sealed class UpdateModeDefinitionCommandValidator : AbstractValidator<UpdateModeDefinitionCommand>
{
    public UpdateModeDefinitionCommandValidator()
    {
        RuleFor(c => c.ModeDefinitionId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(255);
    }
}
