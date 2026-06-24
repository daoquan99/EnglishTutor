using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.TopicPhrases.DeleteTopicPhrase;

public sealed record DeleteTopicPhraseCommand(
    Guid Id,
    Guid TopicId,
    Guid? UserId) : ICommand;

public sealed class DeleteTopicPhraseCommandValidator : AbstractValidator<DeleteTopicPhraseCommand>
{
    public DeleteTopicPhraseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TopicId).NotEmpty();
    }
}
