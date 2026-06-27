using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Errors;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;
using EnglishTutor.Learning.Domain.Shared;

namespace EnglishTutor.Learning.Application.Commands.TopicVocabularies.UpdateTopicVocabulary;

public sealed class UpdateTopicVocabularyCommandHandler : ICommandHandler<UpdateTopicVocabularyCommand>
{
    private readonly ITopicVocabularyRepository _vocabularyRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public UpdateTopicVocabularyCommandHandler(
        ITopicVocabularyRepository vocabularyRepository,
        ITopicRepository topicRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _vocabularyRepository = vocabularyRepository;
        _topicRepository = topicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTopicVocabularyCommand request, CancellationToken cancellationToken)
    {
        var topicActive = await _topicRepository.ExistsActiveTopicAsync(request.TopicId, cancellationToken);
        if (!topicActive)
        {
            return Result.Failure(TopicErrors.NotFound(request.TopicId));
        }

        var vocabulary = await _vocabularyRepository.GetByIdForTopicAsync(
            request.TopicId,
            request.Id,
            includeDeleted: false,
            cancellationToken);

        if (vocabulary is null)
        {
            return Result.Failure(TopicVocabularyErrors.NotFound(request.Id));
        }

        var normalizedWord = TextNormalizer.Normalize(request.Word);
        var duplicateExists = await _vocabularyRepository.ExistsByWordAsync(
            request.TopicId,
            normalizedWord,
            request.Id,
            cancellationToken);

        if (duplicateExists)
        {
            return Result.Failure(TopicVocabularyErrors.DuplicateWord(request.Word));
        }

        vocabulary.Update(
            request.Word,
            request.Definition,
            request.PartOfSpeech,
            request.Phonetic,
            request.ExampleSentence,
            request.ExampleTranslation,
            request.IsActive);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
