using System;
using EnglishTutor.BuildingBlocks.Application.Queries;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Queries.TopicPhrases.GetTopicPhraseById;

public sealed record GetTopicPhraseByIdQuery(
    Guid TopicId,
    Guid Id,
    bool IncludeDeleted) : IQuery<PhraseReadModel>;

public sealed class GetTopicPhraseByIdQueryValidator : AbstractValidator<GetTopicPhraseByIdQuery>
{
    public GetTopicPhraseByIdQueryValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
