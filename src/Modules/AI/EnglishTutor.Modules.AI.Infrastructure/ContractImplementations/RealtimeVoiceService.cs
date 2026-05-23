using System.Collections.Concurrent;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.AI.Infrastructure.Clients;

namespace EnglishTutor.Modules.AI.Infrastructure.ContractImplementations;

public sealed class RealtimeVoiceService(GeminiLiveClient liveClient, IDateTimeProvider dateTimeProvider) : IRealtimeVoiceService
{
    private readonly ConcurrentDictionary<Guid, RealtimeSessionInfo> _sessions = new();

    public async Task<RealtimeSessionInfo> StartSessionAsync(RealtimeSessionConfig config, CancellationToken ct)
    {
        var session = await liveClient.StartSessionAsync(config, ct);
        _sessions[session.SessionId] = session;
        return session;
    }

    public Task SendAudioChunkAsync(Guid sessionId, byte[] audioChunk, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _ = audioChunk;
        EnsureSession(sessionId);
        return Task.CompletedTask;
    }

    public Task<RealtimeMessage> ReceiveMessageAsync(Guid sessionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        EnsureSession(sessionId);
        return Task.FromResult(new RealtimeMessage(sessionId, "text", "Realtime voice provider boundary is ready.", null, dateTimeProvider.UtcNow));
    }

    public Task EndSessionAsync(Guid sessionId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _sessions.TryRemove(sessionId, out _);
        return Task.CompletedTask;
    }

    private void EnsureSession(Guid sessionId)
    {
        if (!_sessions.ContainsKey(sessionId))
        {
            throw new InvalidOperationException("Realtime voice session was not found.");
        }
    }
}
