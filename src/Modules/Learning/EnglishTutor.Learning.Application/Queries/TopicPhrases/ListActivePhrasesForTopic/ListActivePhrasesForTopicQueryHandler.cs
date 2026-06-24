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
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;

namespace EnglishTutor.Learning.Application.Queries.TopicPhrases.ListActivePhrasesForTopic;

public sealed class ListActivePhrasesForTopicQueryHandler : IQueryHandler<ListActivePhrasesForTopicQuery, PagedResult<PhraseReadModel>>
{
    private readonly ITopicPhraseRepository _phraseRepository;
    private readonly ITopicRepository _topicRepository;

    public ListActivePhrasesForTopicQueryHandler(
        ITopicPhraseRepository phraseRepository,
        ITopicRepository topicRepository)
    {
        _phraseRepository = phraseRepository;
        _topicRepository = topicRepository;
    }

    public async Task<Result<PagedResult<PhraseReadModel>>> Handle(ListActivePhrasesForTopicQuery request, CancellationToken cancellationToken)
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
            return Result.Failure<PagedResult<PhraseReadModel>>(
                Guid.TryParse(request.TopicIdOrSlug, out var parsedId) 
                    ? TopicErrors.NotFound(parsedId) 
                    : TopicErrors.NotFoundBySlug(request.TopicIdOrSlug));
        }

        var count = await _phraseRepository.CountByTopicIdAsync(
            topic.Id,
            activeOnly: true,
            includeDeleted: false,
            cancellationToken);

        var items = await _phraseRepository.ListByTopicIdAsync(
            topic.Id,
            request.Page,
            request.PageSize,
            cancellationToken);

        var projected = items
            .Select(p => new PhraseReadModel(
                p.Id,
                p.TopicId,
                p.Phrase,
                p.Translation,
                p.Context,
                p.IsActive))
            .ToList();

        var pagedResult = PagedResult<PhraseReadModel>.Create(projected, count, request.Page, request.PageSize);
        return Result.Success(pagedResult);
    }
}
