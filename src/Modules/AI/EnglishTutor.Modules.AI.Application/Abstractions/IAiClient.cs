using EnglishTutor.Modules.AI.Application.DTOs;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IAiClient
{
    Task<AiResponse> SendAsync(AiRequest request, CancellationToken cancellationToken);
}
