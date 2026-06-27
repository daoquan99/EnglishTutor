using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Errors;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;

namespace EnglishTutor.Learning.Application.Queries.TopicVocabularies.GetTopicVocabularyById;

public sealed class GetTopicVocabularyByIdQueryHandler : IQueryHandler<GetTopicVocabularyByIdQuery, VocabularyReadModel>
{
    private readonly ITopicVocabularyRepository _vocabularyRepository;

    public GetTopicVocabularyByIdQueryHandler(ITopicVocabularyRepository vocabularyRepository)
    {
        _vocabularyRepository = vocabularyRepository;
    }

    public async Task<Result<VocabularyReadModel>> Handle(GetTopicVocabularyByIdQuery request, CancellationToken cancellationToken)
    {
        var vocabulary = await _vocabularyRepository.GetByIdForTopicAsync(
            request.TopicId,
            request.Id,
            request.IncludeDeleted,
            cancellationToken);

        if (vocabulary is null)
        {
            return Result.Failure<VocabularyReadModel>(TopicVocabularyErrors.NotFound(request.Id));
        }

        var readModel = new VocabularyReadModel(
            vocabulary.Id,
            vocabulary.TopicId,
            vocabulary.Word,
            vocabulary.Definition,
            vocabulary.PartOfSpeech,
            vocabulary.Phonetic,
            vocabulary.ExampleSentence,
            vocabulary.ExampleTranslation,
            vocabulary.IsActive);

        return Result.Success(readModel);
    }
}
