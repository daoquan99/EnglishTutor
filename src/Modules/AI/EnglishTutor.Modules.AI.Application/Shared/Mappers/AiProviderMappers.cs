using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Entities;

namespace EnglishTutor.Modules.AI.Application.Shared.Mappers;

internal static class AiProviderMappers
{
    public static AiProviderResponse ToResponse(this AiProvider provider) =>
        new(
            provider.Id,
            provider.ProviderName,
            provider.DisplayName,
            provider.ProviderType.ToString(),
            provider.BaseUrl,
            provider.ApiKeySecretName,
            provider.IsEnabled,
            provider.Models
                .OrderBy(model => model.Capability)
                .ThenBy(model => model.Priority)
                .ThenBy(model => model.ModelCode)
                .Select(ToResponse)
                .ToList());

    public static AiProviderModelResponse ToResponse(this AiProviderModel model) =>
        new(
            model.Id,
            model.ModelCode,
            model.DisplayName,
            model.Capability.ToString(),
            model.IsEnabled,
            model.SupportsStreaming,
            model.MaxInputTokens,
            model.MaxOutputTokens,
            model.CostPerInput1KTokens,
            model.CostPerOutput1KTokens,
            model.Priority);

    public static AiRuntimeRouteResponse ToResponse(this AiRuntimeRoute route) =>
        new(
            route.Id,
            route.TaskType.ToString(),
            route.Capability.ToString(),
            route.PreferredProviderName,
            route.PreferredModelCode,
            route.FallbackProviderName,
            route.FallbackModelCode,
            route.MaxTokens,
            route.Temperature,
            route.IsActive);
}
