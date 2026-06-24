using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Errors;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;
using EnglishTutor.Learning.Domain.Shared;

namespace EnglishTutor.Learning.Application.Commands.TopicPhrases.UpdateTopicPhrase;

public sealed class UpdateTopicPhraseCommandHandler : ICommandHandler<UpdateTopicPhraseCommand>
{
    private readonly ITopicPhraseRepository _phraseRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public UpdateTopicPhraseCommandHandler(
        ITopicPhraseRepository phraseRepository,
        ITopicRepository topicRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _phraseRepository = phraseRepository;
        _topicRepository = topicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTopicPhraseCommand request, CancellationToken cancellationToken)
    {
        var topicActive = await _topicRepository.ExistsActiveTopicAsync(request.TopicId, cancellationToken);
        if (!topicActive)
        {
            return Result.Failure(TopicErrors.NotFound(request.TopicId));
        }

        var phrase = await _phraseRepository.GetByIdForTopicAsync(
            request.TopicId,
            request.Id,
            includeDeleted: false,
            cancellationToken);

        if (phrase is null)
        {
            return Result.Failure(TopicPhraseErrors.NotFound(request.Id));
        }

        var normalizedPhrase = TextNormalizer.Normalize(request.Phrase);
        var duplicateExists = await _phraseRepository.ExistsByPhraseAsync(
            request.TopicId,
            normalizedPhrase,
            request.Id,
            cancellationToken);

        if (duplicateExists)
        {
            return Result.Failure(TopicPhraseErrors.DuplicatePhrase(request.Phrase));
        }

        phrase.Update(
            request.Phrase,
            request.Translation,
            request.Context,
            request.IsActive);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
