using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.Realtime.Application.Abstractions;

namespace EnglishTutor.Realtime.Infrastructure.Registry;

public sealed class InMemoryRealtimeConnectionRegistry : IRealtimeConnectionRegistry
{
    private readonly ConcurrentDictionary<string, RealtimeConnection> _connections = new();
    private readonly IDateTimeProvider _clock;

    public InMemoryRealtimeConnectionRegistry(IDateTimeProvider clock)
    {
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public Task RegisterConnectionAsync(string connectionId, Guid userId, Guid sessionId, CancellationToken ct)
    {
        var now = _clock.UtcNow;
        var conn = new RealtimeConnection(connectionId, userId, sessionId, now, now, "Connected");
        _connections[connectionId] = conn;
        return Task.CompletedTask;
    }

    public Task UnregisterConnectionAsync(string connectionId, Guid sessionId, CancellationToken ct)
    {
        _connections.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }

    public Task UnregisterConnectionFromAllSessionsAsync(string connectionId, CancellationToken ct)
    {
        _connections.TryRemove(connectionId, out _);
        return Task.CompletedTask;
    }

    public Task UpdateHeartbeatAsync(string connectionId, CancellationToken ct)
    {
        if (_connections.TryGetValue(connectionId, out var conn))
        {
            var updated = conn with { LastHeartbeatAtUtc = _clock.UtcNow };
            _connections[connectionId] = updated;
        }
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<RealtimeConnection>> GetSessionConnectionsAsync(Guid sessionId, CancellationToken ct)
    {
        IReadOnlyList<RealtimeConnection> list = _connections.Values
            .Where(x => x.SessionId == sessionId)
            .ToList();
        return Task.FromResult(list);
    }

    public Task<RealtimeConnection?> GetConnectionAsync(string connectionId, CancellationToken ct)
    {
        _connections.TryGetValue(connectionId, out var conn);
        return Task.FromResult(conn);
    }
}
