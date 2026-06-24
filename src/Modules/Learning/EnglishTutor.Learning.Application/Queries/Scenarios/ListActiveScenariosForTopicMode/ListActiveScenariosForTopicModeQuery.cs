using System.Collections.Generic;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Learning.Application.Queries.Scenarios.ListActiveScenariosForTopicMode;

public sealed record ListActiveScenariosForTopicModeQuery(
    string TopicIdOrSlug,
    string ModeIdOrCode) : IQuery<IReadOnlyList<ScenarioReadModel>>;
