namespace EnglishTutor.Modules.AI.Application.Shared.DTOs;

public sealed record AiProviderModelResponse(
    Guid Id,
    string ModelCode,
    string DisplayName,
    string Capability,
    bool IsEnabled,
    bool SupportsStreaming,
    int MaxInputTokens,
    int MaxOutputTokens,
    decimal CostPerInput1KTokens,
    decimal CostPerOutput1KTokens,
    int Priority);
