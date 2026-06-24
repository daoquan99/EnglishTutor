using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.TopicVocabularies.UpdateTopicVocabulary;

public sealed record UpdateTopicVocabularyCommand(
    Guid Id,
    Guid TopicId,
    string Word,
    string Definition,
    string? PartOfSpeech,
    string? Phonetic,
    string? ExampleSentence,
    string? ExampleTranslation,
    bool IsActive,
    Guid? UserId) : ICommand;

public sealed class UpdateTopicVocabularyCommandValidator : AbstractValidator<UpdateTopicVocabularyCommand>
{
    public UpdateTopicVocabularyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Word).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Definition).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.PartOfSpeech).MaximumLength(50);
        RuleFor(x => x.Phonetic).MaximumLength(100);
        RuleFor(x => x.ExampleSentence).MaximumLength(1000);
        RuleFor(x => x.ExampleTranslation).MaximumLength(1000);
    }
}
