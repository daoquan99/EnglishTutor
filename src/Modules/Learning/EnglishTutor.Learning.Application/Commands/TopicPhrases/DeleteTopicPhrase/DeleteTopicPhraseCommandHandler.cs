using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Errors;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;
using EnglishTutor.BuildingBlocks.Application.DateTime;

namespace EnglishTutor.Learning.Application.Commands.TopicPhrases.DeleteTopicPhrase;

public sealed class DeleteTopicPhraseCommandHandler : ICommandHandler<DeleteTopicPhraseCommand>
{
    private readonly ITopicPhraseRepository _phraseRepository;
    private readonly ILearningUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public DeleteTopicPhraseCommandHandler(
        ITopicPhraseRepository phraseRepository,
        ILearningUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _phraseRepository = phraseRepository;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result> Handle(DeleteTopicPhraseCommand request, CancellationToken cancellationToken)
    {
        var phrase = await _phraseRepository.GetByIdForTopicAsync(
            request.TopicId,
            request.Id,
            includeDeleted: false,
            cancellationToken);

        if (phrase is null)
        {
            return Result.Failure(TopicPhraseErrors.NotFound(request.Id));
        }

        phrase.MarkDeleted(request.UserId, _clock.UtcNow);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
