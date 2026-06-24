using System;

namespace EnglishTutor.Learning.Presentation.Dtos;

public sealed record CreateScenarioRequest(
    Guid TopicId,
    Guid ModeDefinitionId,
    string Name,
    string? Description,
    string DifficultyLevel,
    string PromptTemplate);

public sealed record UpdateScenarioRequest(
    string Name,
    string? Description,
    string DifficultyLevel,
    string PromptTemplate);

public sealed record ScenarioResponse(
    Guid Id,
    Guid TopicId,
    Guid ModeDefinitionId,
    string Name,
    string? Description,
    string DifficultyLevel,
    string PromptTemplate,
    bool IsActive);
