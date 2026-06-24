using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;

namespace EnglishTutor.Learning.Application.Queries.TopicPhrases.ListTopicPhrasesAdmin;

public sealed class ListTopicPhrasesAdminQueryHandler : IQueryHandler<ListTopicPhrasesAdminQuery, PagedResult<PhraseReadModel>>
{
    private readonly ITopicPhraseRepository _phraseRepository;
    private readonly ITopicRepository _topicRepository;

    public ListTopicPhrasesAdminQueryHandler(
        ITopicPhraseRepository phraseRepository,
        ITopicRepository topicRepository)
    {
        _phraseRepository = phraseRepository;
        _topicRepository = topicRepository;
    }

    public async Task<Result<PagedResult<PhraseReadModel>>> Handle(ListTopicPhrasesAdminQuery request, CancellationToken cancellationToken)
    {
        var topic = await _topicRepository.GetByIdAsync(request.TopicId, includeDeleted: true, cancellationToken);
        if (topic is null)
        {
            return Result.Failure<PagedResult<PhraseReadModel>>(TopicErrors.NotFound(request.TopicId));
        }

        var count = await _phraseRepository.CountByTopicIdAsync(
            request.TopicId,
            activeOnly: !request.IncludeInactive,
            includeDeleted: request.IncludeDeleted,
            cancellationToken);

        var items = await _phraseRepository.ListByTopicIdAdminAsync(
            request.TopicId,
            request.Page,
            request.PageSize,
            request.IncludeInactive,
            request.IncludeDeleted,
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
