using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.Scenarios.ListAllScenarios;

public sealed record ListAllScenariosQuery : IQuery<IReadOnlyList<ScenarioReadModel>>;
