using System;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Queries.TopicVocabularies.ListActiveVocabularyForTopic;

public sealed record ListActiveVocabularyForTopicQuery(
    string TopicIdOrSlug,
    int Page,
    int PageSize) : IQuery<PagedResult<VocabularyReadModel>>;

public sealed class ListActiveVocabularyForTopicQueryValidator : AbstractValidator<ListActiveVocabularyForTopicQuery>
{
    public ListActiveVocabularyForTopicQueryValidator()
    {
        RuleFor(x => x.TopicIdOrSlug).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
