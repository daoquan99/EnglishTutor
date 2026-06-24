using System;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Queries.TopicPhrases.ListActivePhrasesForTopic;

public sealed record ListActivePhrasesForTopicQuery(
    string TopicIdOrSlug,
    int Page,
    int PageSize) : IQuery<PagedResult<PhraseReadModel>>;

public sealed class ListActivePhrasesForTopicQueryValidator : AbstractValidator<ListActivePhrasesForTopicQuery>
{
    public ListActivePhrasesForTopicQueryValidator()
    {
        RuleFor(x => x.TopicIdOrSlug).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
