using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.DTOs;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class GeminiClient : IAiClient
{
    public Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken)
    {
        // Provider transport is isolated here; deterministic fallback keeps local development runnable before keys exist.
        return Task.FromResult(new AiResponse(request.Prompt, 0, 0, 0));
    }
}
