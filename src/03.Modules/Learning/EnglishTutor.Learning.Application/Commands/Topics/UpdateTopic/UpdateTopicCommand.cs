using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.Topics.UpdateTopic;

public sealed record UpdateTopicCommand(
    Guid TopicId,
    string Name,
    string Slug,
    string? Description,
    Guid? CurrentUserId) : ICommand;

public sealed class UpdateTopicCommandValidator : AbstractValidator<UpdateTopicCommand>
{
    public UpdateTopicCommandValidator()
    {
        RuleFor(c => c.TopicId).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(255);
        RuleFor(c => c.Slug).NotEmpty().MaximumLength(255)
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug must contain only lowercase alphanumeric characters and hyphens.");
    }
}
