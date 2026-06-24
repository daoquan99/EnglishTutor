using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Errors;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;

namespace EnglishTutor.Learning.Application.Queries.Scenarios.GetScenarioById;

public sealed class GetScenarioByIdQueryHandler : IQueryHandler<GetScenarioByIdQuery, ScenarioReadModel>
{
    private readonly IScenarioRepository _scenarioRepository;

    public GetScenarioByIdQueryHandler(IScenarioRepository scenarioRepository)
    {
        _scenarioRepository = scenarioRepository;
    }

    public async Task<Result<ScenarioReadModel>> Handle(GetScenarioByIdQuery request, CancellationToken cancellationToken)
    {
        var scenario = await _scenarioRepository.GetByIdAsync(request.ScenarioId, includeDeleted: false, cancellationToken);
        if (scenario is null)
        {
            return Result.Failure<ScenarioReadModel>(ScenarioErrors.NotFound(request.ScenarioId));
        }

        var readModel = new ScenarioReadModel(
            scenario.Id,
            scenario.TopicId,
            scenario.ModeDefinitionId,
            scenario.Name,
            scenario.Description,
            scenario.DifficultyLevel,
            scenario.PromptTemplate,
            scenario.IsActive);

        return Result.Success(readModel);
    }
}
