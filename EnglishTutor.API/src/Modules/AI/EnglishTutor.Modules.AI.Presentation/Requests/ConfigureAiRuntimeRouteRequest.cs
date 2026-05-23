namespace EnglishTutor.Modules.AI.Presentation.Requests;

public sealed record ConfigureAiRuntimeRouteRequest(
    string Capability,
    string PreferredProviderName,
    string PreferredModelCode,
    string? FallbackProviderName,
    string? FallbackModelCode,
    int MaxTokens,
    decimal Temperature,
    bool IsActive);
