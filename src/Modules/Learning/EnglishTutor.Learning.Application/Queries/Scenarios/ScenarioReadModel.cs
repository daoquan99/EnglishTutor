using System;

namespace EnglishTutor.Learning.Application.Queries.Scenarios;

public sealed record ScenarioReadModel(
    Guid Id,
    Guid TopicId,
    Guid ModeDefinitionId,
    string Name,
    string? Description,
    string DifficultyLevel,
    string PromptTemplate,
    bool IsActive);
