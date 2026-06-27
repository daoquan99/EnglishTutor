using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;

namespace EnglishTutor.Learning.Application.Queries.TopicVocabularies.ListActiveVocabularyForTopic;

public sealed class ListActiveVocabularyForTopicQueryHandler : IQueryHandler<ListActiveVocabularyForTopicQuery, PagedResult<VocabularyReadModel>>
{
    private readonly ITopicVocabularyRepository _vocabularyRepository;
    private readonly ITopicRepository _topicRepository;

    public ListActiveVocabularyForTopicQueryHandler(
        ITopicVocabularyRepository vocabularyRepository,
        ITopicRepository topicRepository)
    {
        _vocabularyRepository = vocabularyRepository;
        _topicRepository = topicRepository;
    }

    public async Task<Result<PagedResult<VocabularyReadModel>>> Handle(ListActiveVocabularyForTopicQuery request, CancellationToken cancellationToken)
    {
        Topic? topic;
        if (Guid.TryParse(request.TopicIdOrSlug, out var topicId))
        {
            topic = await _topicRepository.GetByIdAsync(topicId, includeDeleted: false, cancellationToken);
        }
        else
        {
            topic = await _topicRepository.GetBySlugAsync(request.TopicIdOrSlug, includeDeleted: false, cancellationToken);
        }

        if (topic is null || !topic.IsActive)
        {
            return Result.Failure<PagedResult<VocabularyReadModel>>(
                Guid.TryParse(request.TopicIdOrSlug, out var parsedId) 
                    ? TopicErrors.NotFound(parsedId) 
                    : TopicErrors.NotFoundBySlug(request.TopicIdOrSlug));
        }

        var count = await _vocabularyRepository.CountByTopicIdAsync(
            topic.Id,
            activeOnly: true,
            includeDeleted: false,
            cancellationToken);

        var items = await _vocabularyRepository.ListByTopicIdAsync(
            topic.Id,
            request.Page,
            request.PageSize,
            cancellationToken);

        var projected = items
            .Select(v => new VocabularyReadModel(
                v.Id,
                v.TopicId,
                v.Word,
                v.Definition,
                v.PartOfSpeech,
                v.Phonetic,
                v.ExampleSentence,
                v.ExampleTranslation,
                v.IsActive))
            .ToList();

        var pagedResult = PagedResult<VocabularyReadModel>.Create(projected, count, request.Page, request.PageSize);
        return Result.Success(pagedResult);
    }
}
