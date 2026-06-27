using System;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Learning.Domain.Aggregates.Scenarios.Errors;

public static class ScenarioErrors
{
    public static Error NotFound(Guid scenarioId) =>
        new("Learning.ScenarioNotFound", $"Scenario '{scenarioId}' was not found.");

    public static Error DuplicateName(string name) =>
        new("Learning.ScenarioDuplicateName", $"Scenario with name '{name}' already exists for this Topic + Mode combination.");

    public static Error TopicModeNotEnabled() =>
        new("Learning.ScenarioTopicModeNotEnabled", "The specified Mode is not enabled for this Topic.");
}
