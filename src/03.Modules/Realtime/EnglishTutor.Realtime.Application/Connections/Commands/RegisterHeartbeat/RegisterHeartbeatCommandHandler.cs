using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Realtime.Application.Abstractions;
using EnglishTutor.Realtime.Application.Errors;

namespace EnglishTutor.Realtime.Application.Connections.Commands.RegisterHeartbeat;

internal sealed class RegisterHeartbeatCommandHandler : ICommandHandler<RegisterHeartbeatCommand>
{
    private readonly IRealtimeConnectionRegistry _registry;

    public RegisterHeartbeatCommandHandler(IRealtimeConnectionRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task<Result> Handle(RegisterHeartbeatCommand command, CancellationToken ct)
    {
        var conn = await _registry.GetConnectionAsync(command.ConnectionId, ct);
        if (conn == null)
        {
            return Result.Failure(RealtimeErrors.ConnectionNotFound);
        }

        await _registry.UpdateHeartbeatAsync(command.ConnectionId, ct);
        return Result.Success();
    }
}
