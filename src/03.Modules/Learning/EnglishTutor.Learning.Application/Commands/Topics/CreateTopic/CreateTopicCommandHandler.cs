using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Commands.Topics.CreateTopic;

public sealed class CreateTopicCommandHandler : ICommandHandler<CreateTopicCommand, Guid>
{
    private readonly ITopicRepository _topicRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public CreateTopicCommandHandler(ITopicRepository topicRepository, ILearningUnitOfWork unitOfWork)
    {
        _topicRepository = topicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateTopicCommand request, CancellationToken cancellationToken)
    {
        var slugExists = await _topicRepository.ExistsBySlugAsync(request.Slug, cancellationToken);
        if (slugExists)
        {
            return Result.Failure<Guid>(TopicErrors.DuplicateSlug(request.Slug));
        }

        var topic = Topic.Create(
            request.Name,
            request.Slug,
            request.Description,
            request.CurrentUserId);

        await _topicRepository.AddAsync(topic, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(topic.Id);
    }
}
