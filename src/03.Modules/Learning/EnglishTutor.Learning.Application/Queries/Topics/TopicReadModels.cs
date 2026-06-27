using System;
using System.Collections.Generic;

namespace EnglishTutor.Learning.Application.Queries.Topics;

public sealed record TopicReadModel(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive);

public sealed record TopicModeReadModel(
    Guid Id,
    Guid TopicId,
    Guid ModeDefinitionId,
    string ModeCode,
    string ModeName,
    bool IsEnabled,
    string? ConfigJson);

public sealed record TopicDetailsReadModel(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    bool IsActive,
    IReadOnlyList<TopicModeReadModel> TopicModes);
