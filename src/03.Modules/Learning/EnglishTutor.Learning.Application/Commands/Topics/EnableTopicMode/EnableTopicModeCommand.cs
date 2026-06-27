using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.Topics.EnableTopicMode;

public sealed record EnableTopicModeCommand(
    Guid TopicId,
    Guid ModeDefinitionId,
    string? ConfigJson,
    Guid? CurrentUserId) : ICommand;

public sealed class EnableTopicModeCommandValidator : AbstractValidator<EnableTopicModeCommand>
{
    public EnableTopicModeCommandValidator()
    {
        RuleFor(c => c.TopicId).NotEmpty();
        RuleFor(c => c.ModeDefinitionId).NotEmpty();
    }
}
