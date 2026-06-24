using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Errors;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;
using EnglishTutor.Learning.Domain.Shared;

namespace EnglishTutor.Learning.Application.Commands.TopicVocabularies.AddTopicVocabulary;

public sealed class AddTopicVocabularyCommandHandler : ICommandHandler<AddTopicVocabularyCommand, Guid>
{
    private readonly ITopicVocabularyRepository _vocabularyRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public AddTopicVocabularyCommandHandler(
        ITopicVocabularyRepository vocabularyRepository,
        ITopicRepository topicRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _vocabularyRepository = vocabularyRepository;
        _topicRepository = topicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(AddTopicVocabularyCommand request, CancellationToken cancellationToken)
    {
        var topicActive = await _topicRepository.ExistsActiveTopicAsync(request.TopicId, cancellationToken);
        if (!topicActive)
        {
            return Result.Failure<Guid>(TopicErrors.NotFound(request.TopicId));
        }

        var normalizedWord = TextNormalizer.Normalize(request.Word);
        var duplicateExists = await _vocabularyRepository.ExistsByWordAsync(request.TopicId, normalizedWord, null, cancellationToken);
        if (duplicateExists)
        {
            return Result.Failure<Guid>(TopicVocabularyErrors.DuplicateWord(request.Word));
        }

        var vocabulary = TopicVocabulary.Create(
            request.TopicId,
            request.Word,
            request.Definition,
            request.PartOfSpeech,
            request.Phonetic,
            request.ExampleSentence,
            request.ExampleTranslation);

        _vocabularyRepository.Add(vocabulary);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(vocabulary.Id);
    }
}
