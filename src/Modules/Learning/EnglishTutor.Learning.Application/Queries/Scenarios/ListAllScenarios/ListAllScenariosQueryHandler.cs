using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;

namespace EnglishTutor.Learning.Application.Queries.Scenarios.ListAllScenarios;

public sealed class ListAllScenariosQueryHandler : IQueryHandler<ListAllScenariosQuery, IReadOnlyList<ScenarioReadModel>>
{
    private readonly IScenarioRepository _scenarioRepository;

    public ListAllScenariosQueryHandler(IScenarioRepository scenarioRepository)
    {
        _scenarioRepository = scenarioRepository;
    }

    public async Task<Result<IReadOnlyList<ScenarioReadModel>>> Handle(ListAllScenariosQuery request, CancellationToken cancellationToken)
    {
        var scenarios = await _scenarioRepository.ListAllAsync(includeDeleted: false, cancellationToken);

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
