using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Errors;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Commands.Topics.EnableTopicMode;

public sealed class EnableTopicModeCommandHandler : ICommandHandler<EnableTopicModeCommand>
{
    private readonly ITopicRepository _topicRepository;
    private readonly IModeDefinitionRepository _modeRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public EnableTopicModeCommandHandler(
        ITopicRepository topicRepository,
        IModeDefinitionRepository modeRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _topicRepository = topicRepository;
        _modeRepository = modeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(EnableTopicModeCommand request, CancellationToken cancellationToken)
    {
        var topic = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
        if (topic is null)
        {
            return Result.Failure(TopicErrors.NotFound(request.TopicId));
        }

        var mode = await _modeRepository.GetByIdAsync(request.ModeDefinitionId, cancellationToken);
        if (mode is null)
        {
            return Result.Failure(ModeDefinitionErrors.NotFound(request.ModeDefinitionId));
        }
        if (!mode.IsActive)
        {
            return Result.Failure(Error.Validation("Learning.ModeDefinitionInactive", $"Mode definition '{request.ModeDefinitionId}' is inactive."));
        }

        var newMode = topic.EnableMode(request.ModeDefinitionId, request.ConfigJson, request.CurrentUserId);
        if (newMode is not null)
        {
            _topicRepository.AddTopicMode(newMode);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
