using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Commands.Topics.DisableTopic;

public sealed class DisableTopicCommandHandler : ICommandHandler<DisableTopicCommand>
{
    private readonly ITopicRepository _topicRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public DisableTopicCommandHandler(ITopicRepository topicRepository, ILearningUnitOfWork unitOfWork)
    {
        _topicRepository = topicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DisableTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
        if (topic is null)
        {
            return Result.Failure(TopicErrors.NotFound(request.TopicId));
        }

        topic.Disable(request.CurrentUserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
