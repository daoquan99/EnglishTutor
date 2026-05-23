using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.Shared.DTOs;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class DeepSeekClient(OpenAiCompatibleClient openAiCompatibleClient) : IAiClient
{
    public Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken)
    {
        var deepSeekRequest = request with
        {
            ProviderName = string.IsNullOrWhiteSpace(request.ProviderName) ? "deepseek" : request.ProviderName,
            ProviderType = AiProviderType.DeepSeek
        };

        return openAiCompatibleClient.SendAsync(deepSeekRequest, cancellationToken);
    }
}
