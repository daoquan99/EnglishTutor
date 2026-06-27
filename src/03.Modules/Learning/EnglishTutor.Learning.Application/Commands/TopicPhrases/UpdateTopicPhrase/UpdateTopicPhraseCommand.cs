using System;
using EnglishTutor.BuildingBlocks.Application.Commands;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Commands.TopicPhrases.UpdateTopicPhrase;

public sealed record UpdateTopicPhraseCommand(
    Guid Id,
    Guid TopicId,
    string Phrase,
    string Translation,
    string? Context,
    bool IsActive,
    Guid? UserId) : ICommand;

public sealed class UpdateTopicPhraseCommandValidator : AbstractValidator<UpdateTopicPhraseCommand>
{
    public UpdateTopicPhraseCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Phrase).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Translation).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Context).MaximumLength(1000);
    }
}
