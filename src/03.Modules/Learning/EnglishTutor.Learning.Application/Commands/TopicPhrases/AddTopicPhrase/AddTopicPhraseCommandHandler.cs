using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Errors;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;
using EnglishTutor.Learning.Domain.Shared;

namespace EnglishTutor.Learning.Application.Commands.TopicPhrases.AddTopicPhrase;

public sealed class AddTopicPhraseCommandHandler : ICommandHandler<AddTopicPhraseCommand, Guid>
{
    private readonly ITopicPhraseRepository _phraseRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public AddTopicPhraseCommandHandler(
        ITopicPhraseRepository phraseRepository,
        ITopicRepository topicRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _phraseRepository = phraseRepository;
        _topicRepository = topicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddTopicPhraseCommand request, CancellationToken cancellationToken)
    {
        var topicActive = await _topicRepository.ExistsActiveTopicAsync(request.TopicId, cancellationToken);
        if (!topicActive)
        {
            return Result.Failure<Guid>(TopicErrors.NotFound(request.TopicId));
        }

        var normalizedPhrase = TextNormalizer.Normalize(request.Phrase);
        var duplicateExists = await _phraseRepository.ExistsByPhraseAsync(request.TopicId, normalizedPhrase, null, cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<Guid>(TopicPhraseErrors.DuplicatePhrase(request.Phrase));
        }

        var phrase = TopicPhrase.Create(
            request.TopicId,
            request.Phrase,
            request.Translation,
            request.Context);

        _phraseRepository.Add(phrase);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(phrase.Id);
    }
}
