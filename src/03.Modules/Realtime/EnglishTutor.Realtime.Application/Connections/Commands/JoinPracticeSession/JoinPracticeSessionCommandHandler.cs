using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Realtime.Application.Abstractions;
using EnglishTutor.Realtime.Application.Errors;

namespace EnglishTutor.Realtime.Application.Connections.Commands.JoinPracticeSession;

internal sealed class JoinPracticeSessionCommandHandler : ICommandHandler<JoinPracticeSessionCommand>
{
    private readonly IPracticeModule _practice;
    private readonly IRealtimeConnectionRegistry _registry;
    private readonly IRealtimeNotifier _notifier;

    public JoinPracticeSessionCommandHandler(
        IPracticeModule practice,
        IRealtimeConnectionRegistry registry,
        IRealtimeNotifier notifier)
    {
        _practice = practice ?? throw new ArgumentNullException(nameof(practice));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    public async Task<Result> Handle(JoinPracticeSessionCommand command, CancellationToken ct)
    {
        var sessionResult = await _practice.GetSessionAsync(command.UserId, command.SessionId, ct);
        if (sessionResult.Status != PracticeQueryStatus.Success || sessionResult.Session == null)
        {
            return Result.Failure(RealtimeErrors.SessionAccessDenied);
        }

        await _registry.RegisterConnectionAsync(command.ConnectionId, command.UserId, command.SessionId, ct);

        // Notify that the session has started / joined
        await _notifier.NotifySessionStartedAsync(
            command.SessionId,
            null,
            sessionResult.Session.TopicCode,
            sessionResult.Session.ModeCode,
            ct);

        return Result.Success();
    }
}
