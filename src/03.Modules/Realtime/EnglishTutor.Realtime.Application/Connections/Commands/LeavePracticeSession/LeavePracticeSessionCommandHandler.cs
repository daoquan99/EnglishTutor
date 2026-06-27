using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Realtime.Application.Abstractions;

namespace EnglishTutor.Realtime.Application.Connections.Commands.LeavePracticeSession;

internal sealed class LeavePracticeSessionCommandHandler : ICommandHandler<LeavePracticeSessionCommand>
{
    private readonly IRealtimeConnectionRegistry _registry;

    public LeavePracticeSessionCommandHandler(IRealtimeConnectionRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    public async Task<Result> Handle(LeavePracticeSessionCommand command, CancellationToken ct)
    {
        await _registry.UnregisterConnectionAsync(command.ConnectionId, command.SessionId, ct);
        return Result.Success();
    }
}
