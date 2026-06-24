using System;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Queries.TopicPhrases.ListTopicPhrasesAdmin;

public sealed record ListTopicPhrasesAdminQuery(
    Guid TopicId,
    int Page,
    int PageSize,
    bool IncludeInactive,
    bool IncludeDeleted) : IQuery<PagedResult<PhraseReadModel>>;

public sealed class ListTopicPhrasesAdminQueryValidator : AbstractValidator<ListTopicPhrasesAdminQuery>
{
    public ListTopicPhrasesAdminQueryValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
