using System;

namespace EnglishTutor.Learning.Presentation.Dtos;

public sealed record CreateModeDefinitionRequest(string Code, string Name, string? Description);
public sealed record UpdateModeDefinitionRequest(string Name, string? Description);

public sealed record ModeDefinitionResponse(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive);
