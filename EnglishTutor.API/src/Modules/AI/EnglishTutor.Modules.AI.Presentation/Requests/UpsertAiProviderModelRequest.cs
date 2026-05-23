namespace EnglishTutor.Modules.AI.Presentation.Requests;

public sealed record UpsertAiProviderModelRequest(
    string ModelCode,
    string DisplayName,
    string Capability,
    bool SupportsStreaming,
    int MaxInputTokens,
    int MaxOutputTokens,
    decimal CostPerInput1KTokens,
    decimal CostPerOutput1KTokens,
    int Priority,
    bool IsEnabled);
