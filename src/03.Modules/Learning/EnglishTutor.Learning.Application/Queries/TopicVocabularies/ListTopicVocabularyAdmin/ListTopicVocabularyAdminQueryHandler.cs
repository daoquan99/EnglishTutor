using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;

namespace EnglishTutor.Learning.Application.Queries.TopicVocabularies.ListTopicVocabularyAdmin;

public sealed class ListTopicVocabularyAdminQueryHandler : IQueryHandler<ListTopicVocabularyAdminQuery, PagedResult<VocabularyReadModel>>
{
    private readonly ITopicVocabularyRepository _vocabularyRepository;
    private readonly ITopicRepository _topicRepository;

    public ListTopicVocabularyAdminQueryHandler(
        ITopicVocabularyRepository vocabularyRepository,
        ITopicRepository topicRepository)
    {
        _vocabularyRepository = vocabularyRepository;
        _topicRepository = topicRepository;
    }

    public async Task<Result<PagedResult<VocabularyReadModel>>> Handle(ListTopicVocabularyAdminQuery request, CancellationToken cancellationToken)
    {
        var topic = await _topicRepository.GetByIdAsync(request.TopicId, includeDeleted: true, cancellationToken);
        if (topic is null)
        {
            return Result.Failure<PagedResult<VocabularyReadModel>>(TopicErrors.NotFound(request.TopicId));
        }

        var count = await _vocabularyRepository.CountByTopicIdAsync(
            request.TopicId,
            activeOnly: !request.IncludeInactive,
            includeDeleted: request.IncludeDeleted,
            cancellationToken);

        var items = await _vocabularyRepository.ListByTopicIdAdminAsync(
            request.TopicId,
            request.Page,
            request.PageSize,
            request.IncludeInactive,
            request.IncludeDeleted,
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
