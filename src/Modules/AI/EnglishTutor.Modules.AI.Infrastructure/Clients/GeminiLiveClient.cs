using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.AI.Contracts.DTOs;

namespace EnglishTutor.Modules.AI.Infrastructure.Clients;

public sealed class GeminiLiveClient(IDateTimeProvider dateTimeProvider)
{
    public Task<RealtimeSessionInfo> StartSessionAsync(RealtimeSessionConfig config, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _ = config;
        return Task.FromResult(new RealtimeSessionInfo(Guid.NewGuid(), dateTimeProvider.UtcNow));
    }
}
