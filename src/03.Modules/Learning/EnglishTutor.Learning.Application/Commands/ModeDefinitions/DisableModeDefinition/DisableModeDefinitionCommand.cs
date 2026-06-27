using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.ModeDefinitions.DisableModeDefinition;

public sealed record DisableModeDefinitionCommand(
    Guid ModeDefinitionId,
    Guid? CurrentUserId) : ICommand;

public sealed class DisableModeDefinitionCommandValidator : AbstractValidator<DisableModeDefinitionCommand>
{
    public DisableModeDefinitionCommandValidator()
    {
        RuleFor(c => c.ModeDefinitionId).NotEmpty();
    }
}
