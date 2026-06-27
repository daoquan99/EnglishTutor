using System;

namespace EnglishTutor.Learning.Application.Queries.ModeDefinitions;

public sealed record ModeDefinitionReadModel(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive);
