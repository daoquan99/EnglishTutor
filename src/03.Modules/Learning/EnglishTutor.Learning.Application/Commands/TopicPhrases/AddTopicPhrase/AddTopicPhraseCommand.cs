using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.TopicPhrases.AddTopicPhrase;

public sealed record AddTopicPhraseCommand(
    Guid TopicId,
    string Phrase,
    string Translation,
    string? Context,
    Guid? UserId) : ICommand<Guid>;

public sealed class AddTopicPhraseCommandValidator : AbstractValidator<AddTopicPhraseCommand>
{
    public AddTopicPhraseCommandValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Phrase).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Translation).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Context).MaximumLength(1000);
    }
}
