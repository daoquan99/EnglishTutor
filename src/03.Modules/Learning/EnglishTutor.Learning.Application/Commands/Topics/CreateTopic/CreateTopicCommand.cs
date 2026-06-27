using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.Topics.CreateTopic;

public sealed record CreateTopicCommand(
    string Name,
    string Slug,
    string? Description,
    Guid? CurrentUserId) : ICommand<Guid>;

public sealed class CreateTopicCommandValidator : AbstractValidator<CreateTopicCommand>
{
    public CreateTopicCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(255);
        RuleFor(c => c.Slug).NotEmpty().MaximumLength(255)
            .Matches(@"^[a-z0-9]+(-[a-z0-9]+)*$")
            .WithMessage("Slug must contain only lowercase alphanumeric characters and hyphens.");
    }
}
