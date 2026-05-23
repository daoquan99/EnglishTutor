using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Entities;
using EnglishTutor.Modules.AI.Domain.Enums;
using EnglishTutor.Modules.AI.Infrastructure.Seed;

namespace EnglishTutor.Modules.AI.Infrastructure.Routing;

public sealed class AiRuntimeRouter(
    IAiRuntimeRouteRepository routeRepository,
    IAiProviderRepository providerRepository,
    IModelRoutingRuleRepository legacyRoutingRuleRepository) : IAiRuntimeRouter
{
    public async Task<AiRuntimeRouteResolution> ResolveAsync(
        AiTaskType taskType,
        AiCapabilityType capability,
        CancellationToken cancellationToken)
    {
        var route = await routeRepository.GetActiveByTaskTypeAsync(taskType, capability, cancellationToken);
        if (route is not null)
        {
            var preferredProvider = await providerRepository.GetByNameAsync(route.PreferredProviderName, cancellationToken);
            var fallbackProvider = route.FallbackProviderName is null
                ? null
                : await providerRepository.GetByNameAsync(route.FallbackProviderName, cancellationToken);

            return new AiRuntimeRouteResolution(
                route.TaskType,
                route.Capability,
                route.PreferredProviderName,
                route.PreferredModelCode,
                preferredProvider?.ProviderType ?? ResolveProviderType(route.PreferredProviderName) ?? AiProviderType.Custom,
                preferredProvider?.BaseUrl,
                preferredProvider?.ApiKeySecretName,
                route.FallbackProviderName,
                route.FallbackModelCode,
                fallbackProvider?.ProviderType ?? ResolveProviderType(route.FallbackProviderName),
                fallbackProvider?.BaseUrl,
                fallbackProvider?.ApiKeySecretName,
                route.MaxTokens,
                route.Temperature,
                ResolveLegacyModel(route.PreferredProviderName, route.PreferredModelCode));
        }

        var legacyRule = await legacyRoutingRuleRepository.GetActiveByTaskTypeAsync(taskType, cancellationToken)
            ?? AiSeedData.CreateDefaultRoutingRule(taskType);

        return FromLegacyRule(taskType, capability, legacyRule);
    }

    private static AiRuntimeRouteResolution FromLegacyRule(
        AiTaskType taskType,
        AiCapabilityType capability,
        ModelRoutingRule legacyRule)
    {
        var (preferredProvider, preferredModel) = ResolveProviderModel(legacyRule.PreferredModel);
        var (fallbackProvider, fallbackModel) = ResolveProviderModel(legacyRule.FallbackModel);

        return new AiRuntimeRouteResolution(
            taskType,
            capability,
            preferredProvider,
            preferredModel,
            ResolveProviderType(preferredProvider) ?? AiProviderType.Custom,
            null,
            null,
            fallbackProvider,
            fallbackModel,
            ResolveProviderType(fallbackProvider),
            null,
            null,
            legacyRule.MaxTokens,
            legacyRule.Temperature,
            legacyRule.PreferredModel);
    }

    private static (string ProviderName, string ModelCode) ResolveProviderModel(AiModelType modelType) =>
        modelType switch
        {
            AiModelType.Gemma => ("local", "gemma"),
            AiModelType.GeminiPro => ("google", "gemini-pro"),
            AiModelType.GeminiLive => ("google", "gemini-live"),
            AiModelType.GeminiTTS => ("google", "gemini-tts"),
            _ => ("google", "gemini-flash")
        };

    private static AiProviderType? ResolveProviderType(string? providerName) =>
        providerName?.Trim().ToLowerInvariant() switch
        {
            "google" => AiProviderType.Google,
            "openai" => AiProviderType.OpenAI,
            "deepseek" => AiProviderType.DeepSeek,
            "openai-compatible" => AiProviderType.OpenAiCompatible,
            "local" => AiProviderType.Local,
            "custom" => AiProviderType.Custom,
            _ => null
        };

    private static AiModelType ResolveLegacyModel(string providerName, string modelCode)
    {
        var normalizedProvider = providerName.Trim().ToLowerInvariant();
        var normalizedModel = modelCode.Trim().ToLowerInvariant();

        if (normalizedProvider == "local" || normalizedModel.Contains("gemma", StringComparison.Ordinal))
        {
            return AiModelType.Gemma;
        }

        if (normalizedModel.Contains("live", StringComparison.Ordinal))
        {
            return AiModelType.GeminiLive;
        }

        if (normalizedModel.Contains("tts", StringComparison.Ordinal))
        {
            return AiModelType.GeminiTTS;
        }

        if (normalizedModel.Contains("pro", StringComparison.Ordinal))
        {
            return AiModelType.GeminiPro;
        }

        return AiModelType.GeminiFlash;
    }
}
