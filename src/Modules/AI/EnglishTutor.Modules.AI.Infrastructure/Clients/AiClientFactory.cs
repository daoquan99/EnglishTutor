using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class AiClientFactory(
    GeminiClient geminiClient,
    GemmaClient gemmaClient) : IAiClient
{
    public Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken)
    {
        IAiClient client = request.Model switch
        {
            AiModelType.Gemma => gemmaClient,
            AiModelType.GeminiFlash or AiModelType.GeminiPro or AiModelType.GeminiLive or AiModelType.GeminiTTS => geminiClient,
            _ => geminiClient
        };

        return client.SendAsync(request, cancellationToken);
    }
}
