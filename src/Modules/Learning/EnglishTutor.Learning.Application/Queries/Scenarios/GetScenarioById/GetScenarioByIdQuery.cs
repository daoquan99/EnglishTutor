using System;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.Scenarios.GetScenarioById;

public sealed record GetScenarioByIdQuery(Guid ScenarioId) : IQuery<ScenarioReadModel>;
