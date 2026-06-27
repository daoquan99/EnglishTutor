using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Errors;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;

namespace EnglishTutor.Learning.Application.Queries.TopicPhrases.GetTopicPhraseById;

public sealed class GetTopicPhraseByIdQueryHandler : IQueryHandler<GetTopicPhraseByIdQuery, PhraseReadModel>
{
    private readonly ITopicPhraseRepository _phraseRepository;

    public GetTopicPhraseByIdQueryHandler(ITopicPhraseRepository phraseRepository)
    {
        _phraseRepository = phraseRepository;
    }

    public async Task<Result<PhraseReadModel>> Handle(GetTopicPhraseByIdQuery request, CancellationToken cancellationToken)
    {
        var phrase = await _phraseRepository.GetByIdForTopicAsync(
            request.TopicId,
            request.Id,
            request.IncludeDeleted,
            cancellationToken);

        if (phrase is null)
        {
            return Result.Failure<PhraseReadModel>(TopicPhraseErrors.NotFound(request.Id));
        }

        var readModel = new PhraseReadModel(
            phrase.Id,
            phrase.TopicId,
            phrase.Phrase,
            phrase.Translation,
            phrase.Context,
            phrase.IsActive);

        return Result.Success(readModel);
    }
}
