using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Errors;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Commands.Scenarios.CreateScenario;

public sealed class CreateScenarioCommandHandler : ICommandHandler<CreateScenarioCommand, Guid>
{
    private readonly ITopicRepository _topicRepository;
    private readonly IModeDefinitionRepository _modeRepository;
    private readonly IScenarioRepository _scenarioRepository;
    private readonly ILearningUnitOfWork _unitOfWork;

    public CreateScenarioCommandHandler(
        ITopicRepository topicRepository,
        IModeDefinitionRepository modeRepository,
        IScenarioRepository scenarioRepository,
        ILearningUnitOfWork unitOfWork)
    {
        _topicRepository = topicRepository;
        _modeRepository = modeRepository;
        _scenarioRepository = scenarioRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateScenarioCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate Topic existence and state
        var topic = await _topicRepository.GetByIdAsync(request.TopicId, cancellationToken);
        if (topic is null)
        {
            return Result.Failure<Guid>(TopicErrors.NotFound(request.TopicId));
        }
        if (!topic.IsActive)
        {
            return Result.Failure<Guid>(Error.Validation("Learning.TopicInactive", $"Topic '{request.TopicId}' is inactive."));
        }

        // 2. Validate ModeDefinition existence and state
        var mode = await _modeRepository.GetByIdAsync(request.ModeDefinitionId, cancellationToken);
        if (mode is null)
        {
            return Result.Failure<Guid>(ModeDefinitionErrors.NotFound(request.ModeDefinitionId));
        }
        if (!mode.IsActive)
        {
            return Result.Failure<Guid>(Error.Validation("Learning.ModeDefinitionInactive", $"Mode definition '{request.ModeDefinitionId}' is inactive."));
        }

        // 3. Verify Topic has enabled the Mode
        var isModeEnabled = topic.TopicModes.Any(tm => tm.ModeDefinitionId == request.ModeDefinitionId && tm.IsEnabled);
        if (!isModeEnabled)
        {
            return Result.Failure<Guid>(ScenarioErrors.TopicModeNotEnabled());
        }

        // 4. Verify Scenario Name is unique for the Topic + Mode combination
        var nameExists = await _scenarioRepository.ExistsByNameAsync(
            request.TopicId,
            request.ModeDefinitionId,
            request.Name,
            cancellationToken);

        if (nameExists)
        {
            return Result.Failure<Guid>(ScenarioErrors.DuplicateName(request.Name));
        }

        // 5. Build and save Scenario aggregate
        var scenario = Scenario.Create(
            request.TopicId,
            request.ModeDefinitionId,
            request.Name,
            request.Description,
            request.DifficultyLevel,
            request.PromptTemplate,
            request.CurrentUserId);

        await _scenarioRepository.AddAsync(scenario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(scenario.Id);
    }
}
