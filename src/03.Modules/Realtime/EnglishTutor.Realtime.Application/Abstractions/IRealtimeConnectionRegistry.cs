using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Realtime.Application.Abstractions;

public sealed record RealtimeConnection(
    string ConnectionId,
    Guid UserId,
    Guid SessionId,
    DateTime JoinedAtUtc,
    DateTime LastHeartbeatAtUtc,
    string ConnectionState);

public interface IRealtimeConnectionRegistry
{
    Task RegisterConnectionAsync(string connectionId, Guid userId, Guid sessionId, CancellationToken ct);
    Task UnregisterConnectionAsync(string connectionId, Guid sessionId, CancellationToken ct);
    Task UnregisterConnectionFromAllSessionsAsync(string connectionId, CancellationToken ct);
    Task UpdateHeartbeatAsync(string connectionId, CancellationToken ct);
    Task<IReadOnlyList<RealtimeConnection>> GetSessionConnectionsAsync(Guid sessionId, CancellationToken ct);
    Task<RealtimeConnection?> GetConnectionAsync(string connectionId, CancellationToken ct);
}
