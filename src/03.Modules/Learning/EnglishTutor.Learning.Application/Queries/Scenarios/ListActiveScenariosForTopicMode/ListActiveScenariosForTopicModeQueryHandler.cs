using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Errors;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Topics;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;

namespace EnglishTutor.Learning.Application.Queries.Scenarios.ListActiveScenariosForTopicMode;

public sealed class ListActiveScenariosForTopicModeQueryHandler : IQueryHandler<ListActiveScenariosForTopicModeQuery, IReadOnlyList<ScenarioReadModel>>
{
    private readonly ITopicRepository _topicRepository;
    private readonly IModeDefinitionRepository _modeRepository;
    private readonly IScenarioRepository _scenarioRepository;

    public ListActiveScenariosForTopicModeQueryHandler(
        ITopicRepository topicRepository,
        IModeDefinitionRepository modeRepository,
        IScenarioRepository scenarioRepository)
    {
        _topicRepository = topicRepository;
        _modeRepository = modeRepository;
        _scenarioRepository = scenarioRepository;
    }

    public async Task<Result<IReadOnlyList<ScenarioReadModel>>> Handle(ListActiveScenariosForTopicModeQuery request, CancellationToken cancellationToken)
    {
        // 1. Resolve Topic
        Topic? topic = null;
        if (Guid.TryParse(request.TopicIdOrSlug, out var topicId))
        {
            topic = await _topicRepository.GetByIdAsync(topicId, includeDeleted: false, cancellationToken);
        }
        else
        {
            topic = await _topicRepository.GetBySlugAsync(request.TopicIdOrSlug, includeDeleted: false, cancellationToken);
        }

        if (topic is null || !topic.IsActive)
        {
            return Result.Failure<IReadOnlyList<ScenarioReadModel>>(
                Guid.TryParse(request.TopicIdOrSlug, out var id) 
                    ? TopicErrors.NotFound(id) 
                    : TopicErrors.NotFoundBySlug(request.TopicIdOrSlug));
        }

        // 2. Resolve ModeDefinition
        ModeDefinition? mode = null;
        if (Guid.TryParse(request.ModeIdOrCode, out var modeId))
        {
            mode = await _modeRepository.GetByIdAsync(modeId, includeDeleted: false, cancellationToken);
        }
        else
        {
            mode = await _modeRepository.GetByCodeAsync(request.ModeIdOrCode, includeDeleted: false, cancellationToken);
        }

        if (mode is null || !mode.IsActive)
        {
            return Result.Failure<IReadOnlyList<ScenarioReadModel>>(
                Guid.TryParse(request.ModeIdOrCode, out var mId) 
                    ? ModeDefinitionErrors.NotFound(mId) 
                    : ModeDefinitionErrors.NotFoundByCode(request.ModeIdOrCode));
        }

        // 3. Verify Topic has enabled the Mode
        var isModeEnabled = topic.TopicModes.Any(tm => tm.ModeDefinitionId == mode.Id && tm.IsEnabled);
        if (!isModeEnabled)
        {
            return Result.Failure<IReadOnlyList<ScenarioReadModel>>(ScenarioErrors.TopicModeNotEnabled());
        }

        // 4. Query active scenarios for topic/mode
        var scenarios = await _scenarioRepository.ListActiveForTopicModeAsync(topic.Id, mode.Id, cancellationToken);

        IReadOnlyList<ScenarioReadModel> result = scenarios
            .Select(s => new ScenarioReadModel(
                s.Id,
                s.TopicId,
                s.ModeDefinitionId,
                s.Name,
                s.Description,
                s.DifficultyLevel,
                s.PromptTemplate,
                s.IsActive))
            .ToList();

        return Result.Success(result);
    }
}
