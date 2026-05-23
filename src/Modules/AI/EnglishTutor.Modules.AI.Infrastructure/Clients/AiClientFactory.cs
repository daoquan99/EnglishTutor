using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class AiClientFactory(
    GeminiClient geminiClient,
    GemmaClient gemmaClient,
    OpenAiCompatibleClient openAiCompatibleClient,
    DeepSeekClient deepSeekClient,
    Microsoft.Extensions.Logging.ILogger<AiClientFactory> logger) : IAiClient
{
    public Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken)
    {
        var providerName = request.ProviderName?.Trim().ToLowerInvariant();
        IAiClient client = request.ProviderType switch
        {
            AiProviderType.OpenAI or AiProviderType.OpenAiCompatible or AiProviderType.Custom => openAiCompatibleClient,
            AiProviderType.DeepSeek => deepSeekClient,
            AiProviderType.Local => gemmaClient,
            AiProviderType.Google => geminiClient,
            _ => ResolveByProviderName(providerName)
        };

        return client.SendAsync(request, cancellationToken);

        IAiClient ResolveByProviderName(string? normalizedProviderName) => normalizedProviderName switch
        {
            "openai" or "openai-compatible" or "custom" => openAiCompatibleClient,
            "deepseek" => deepSeekClient,
            "local" => gemmaClient,
            "google" => geminiClient,
            _ => ResolveByLegacyModel(request.Model, normalizedProviderName)
        };

        IAiClient ResolveByLegacyModel(AiModelType modelType, string? unknownProviderName)
        {
            if (!string.IsNullOrWhiteSpace(unknownProviderName))
            {
                logger.LogWarning("Unknown AI provider {ProviderName}; falling back from legacy model {ModelType}.", unknownProviderName, modelType);
            }

            return modelType switch
            {
                AiModelType.Gemma => gemmaClient,
                AiModelType.GeminiFlash or AiModelType.GeminiPro or AiModelType.GeminiLive or AiModelType.GeminiTTS => geminiClient,
                _ => geminiClient
            };
        }
    }
}
