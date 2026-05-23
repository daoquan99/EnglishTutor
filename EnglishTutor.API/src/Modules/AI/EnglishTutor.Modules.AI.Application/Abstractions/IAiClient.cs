using EnglishTutor.Modules.AI.Application.Shared.DTOs;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IAiClient
{
    Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken);
}
