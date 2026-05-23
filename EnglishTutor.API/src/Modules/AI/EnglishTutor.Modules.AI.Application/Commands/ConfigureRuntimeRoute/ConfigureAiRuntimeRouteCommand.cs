using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;

namespace EnglishTutor.Modules.AI.Application.Commands.ConfigureRuntimeRoute;

public sealed record ConfigureAiRuntimeRouteCommand(
    string TaskType,
    string Capability,
    string PreferredProviderName,
    string PreferredModelCode,
    string? FallbackProviderName,
    string? FallbackModelCode,
    int MaxTokens,
    decimal Temperature,
    bool IsActive) : ICommand<AiRuntimeRouteResponse>;
