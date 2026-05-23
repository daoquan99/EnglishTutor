using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;

namespace EnglishTutor.Modules.AI.Application.Commands.UpsertProviderModel;

public sealed record UpsertAiProviderModelCommand(
    string ProviderName,
    string ModelCode,
    string DisplayName,
    string Capability,
    bool SupportsStreaming,
    int MaxInputTokens,
    int MaxOutputTokens,
    decimal CostPerInput1KTokens,
    decimal CostPerOutput1KTokens,
    int Priority,
    bool IsEnabled) : ICommand<AiProviderResponse>;
