using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.Topics.DisableTopic;

public sealed record DisableTopicCommand(
    Guid TopicId,
    Guid? CurrentUserId) : ICommand;

public sealed class DisableTopicCommandValidator : AbstractValidator<DisableTopicCommand>
{
    public DisableTopicCommandValidator()
    {
        RuleFor(c => c.TopicId).NotEmpty();
    }
}
