using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.TopicVocabularies.DeleteTopicVocabulary;

public sealed record DeleteTopicVocabularyCommand(
    Guid Id,
    Guid TopicId,
    Guid? UserId) : ICommand;

public sealed class DeleteTopicVocabularyCommandValidator : AbstractValidator<DeleteTopicVocabularyCommand>
{
    public DeleteTopicVocabularyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TopicId).NotEmpty();
    }
}
