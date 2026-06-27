using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Commands.Topics.UpdateTopic;

public sealed class UpdateTopicCommandHandler : ICommandHandler<UpdateTopicCommand>
{
    private readonly ITopicRepository _topicRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public UpdateTopicCommandHandler(ITopicRepository topicRepository, ILearningUnitOfWork unitOfWork)
    {
        _topicRepository = topicRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateTopicCommand request, CancellationToken cancellationToken)
    {
        var topic = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
        if (topic is null)
        {
            return Result.Failure(TopicErrors.NotFound(request.TopicId));
        }

        if (topic.Slug.Value != request.Slug.ToLowerInvariant().Trim())
        {
            var slugExists = await _topicRepository.ExistsBySlugAsync(request.Slug, cancellationToken);
            if (slugExists)
            {
                return Result.Failure(TopicErrors.DuplicateSlug(request.Slug));
            }
        }

        topic.UpdateDetails(
            request.Name,
            request.Slug,
            request.Description,
            request.CurrentUserId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
