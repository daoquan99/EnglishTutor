using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.TopicVocabularies.AddTopicVocabulary;

public sealed record AddTopicVocabularyCommand(
    Guid TopicId,
    string Word,
    string Definition,
    string? PartOfSpeech,
    string? Phonetic,
    string? ExampleSentence,
    string? ExampleTranslation,
    Guid? UserId) : ICommand<Guid>;

public sealed class AddTopicVocabularyCommandValidator : AbstractValidator<AddTopicVocabularyCommand>
{
    public AddTopicVocabularyCommandValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Word).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Definition).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.PartOfSpeech).MaximumLength(50);
        RuleFor(x => x.Phonetic).MaximumLength(100);
        RuleFor(x => x.ExampleSentence).MaximumLength(1000);
        RuleFor(x => x.ExampleTranslation).MaximumLength(1000);
    }
}
