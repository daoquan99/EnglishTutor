using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Application.DTOs;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class GemmaClient : IAiClient
{
    public Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken) =>
        Task.FromResult(new AiResponse(request.Prompt, 0, 0, 0));
}
