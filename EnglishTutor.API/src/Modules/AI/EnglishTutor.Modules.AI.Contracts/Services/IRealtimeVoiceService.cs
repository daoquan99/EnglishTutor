using EnglishTutor.Modules.AI.Contracts.DTOs;

namespace EnglishTutor.Modules.AI.Contracts.Services;

public interface IRealtimeVoiceService
{
    Task<RealtimeSessionInfo> StartSessionAsync(RealtimeSessionConfig config, CancellationToken ct);
    Task SendAudioChunkAsync(Guid sessionId, byte[] audioChunk, CancellationToken ct);
    Task<RealtimeMessage> ReceiveMessageAsync(Guid sessionId, CancellationToken ct);
    Task EndSessionAsync(Guid sessionId, CancellationToken ct);
}
