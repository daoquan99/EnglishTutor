using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.ModeDefinitions.CreateModeDefinition;

public sealed record CreateModeDefinitionCommand(
    string Code,
    string Name,
    string? Description,
    Guid? CurrentUserId) : ICommand<Guid>;

public sealed class CreateModeDefinitionCommandValidator : AbstractValidator<CreateModeDefinitionCommand>
{
    public CreateModeDefinitionCommandValidator()
    {
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50)
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Code must contain only lowercase alphanumeric characters and hyphens.");
        RuleFor(c => c.Name).NotEmpty().MaximumLength(255);
    }
}
