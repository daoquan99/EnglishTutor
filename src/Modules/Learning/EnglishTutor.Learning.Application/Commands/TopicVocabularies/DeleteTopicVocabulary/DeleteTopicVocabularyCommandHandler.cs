using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Errors;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;

namespace EnglishTutor.Learning.Application.Commands.TopicVocabularies.DeleteTopicVocabulary;

public sealed class DeleteTopicVocabularyCommandHandler : ICommandHandler<DeleteTopicVocabularyCommand>
{
    private readonly ITopicVocabularyRepository _vocabularyRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public DeleteTopicVocabularyCommandHandler(
        ITopicVocabularyRepository vocabularyRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _vocabularyRepository = vocabularyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteTopicVocabularyCommand request, CancellationToken cancellationToken)
    {
        var vocabulary = await _vocabularyRepository.GetByIdForTopicAsync(
            request.TopicId,
            request.Id,
            includeDeleted: false,
            cancellationToken);

        if (vocabulary is null)
        {
            return Result.Failure(TopicVocabularyErrors.NotFound(request.Id));
        }

        vocabulary.MarkDeleted(request.UserId, DateTime.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
