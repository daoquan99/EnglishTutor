using System;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Queries.TopicVocabularies.ListTopicVocabularyAdmin;

public sealed record ListTopicVocabularyAdminQuery(
    Guid TopicId,
    int Page,
    int PageSize,
    bool IncludeInactive,
    bool IncludeDeleted) : IQuery<PagedResult<VocabularyReadModel>>;

public sealed class ListTopicVocabularyAdminQueryValidator : AbstractValidator<ListTopicVocabularyAdminQuery>
{
    public ListTopicVocabularyAdminQueryValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}
