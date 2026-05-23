namespace EnglishTutor.Modules.AI.Application.Shared.DTOs;

public sealed record AiRuntimeRouteResponse(
    Guid Id,
    string TaskType,
    string Capability,
    string PreferredProviderName,
    string PreferredModelCode,
    string? FallbackProviderName,
    string? FallbackModelCode,
    int MaxTokens,
    decimal Temperature,
    bool IsActive);
