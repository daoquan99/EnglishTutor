using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.Topics.DisableTopicMode;

public sealed record DisableTopicModeCommand(
    Guid TopicId,
    Guid ModeDefinitionId,
    Guid? CurrentUserId) : ICommand;

public sealed class DisableTopicModeCommandValidator : AbstractValidator<DisableTopicModeCommand>
{
    public DisableTopicModeCommandValidator()
    {
        RuleFor(c => c.TopicId).NotEmpty();
        RuleFor(c => c.ModeDefinitionId).NotEmpty();
    }
}
