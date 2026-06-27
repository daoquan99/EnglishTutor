using System;
using EnglishTutor.BuildingBlocks.Application.Queries;
using FluentValidation;

namespace EnglishTutor.Learning.Application.Queries.TopicVocabularies.GetTopicVocabularyById;

public sealed record GetTopicVocabularyByIdQuery(
    Guid TopicId,
    Guid Id,
    bool IncludeDeleted) : IQuery<VocabularyReadModel>;

public sealed class GetTopicVocabularyByIdQueryValidator : AbstractValidator<GetTopicVocabularyByIdQuery>
{
    public GetTopicVocabularyByIdQueryValidator()
    {
        RuleFor(x => x.TopicId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
