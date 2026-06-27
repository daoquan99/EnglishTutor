using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Realtime.Application.Abstractions;
using EnglishTutor.Realtime.Application.Errors;

namespace EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptFinal;

internal sealed class PublishUserTranscriptFinalCommandHandler : ICommandHandler<PublishUserTranscriptFinalCommand>
{
    private readonly IPracticeModule _practice;
    private readonly IRealtimeNotifier _notifier;
    private readonly IDateTimeProvider _clock;

    public PublishUserTranscriptFinalCommandHandler(
        IPracticeModule practice,
        IRealtimeNotifier notifier,
        IDateTimeProvider clock)
    {
        _practice = practice ?? throw new ArgumentNullException(nameof(practice));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
    }

    public async Task<Result> Handle(PublishUserTranscriptFinalCommand command, CancellationToken ct)
    {
        var sessionResult = await _practice.GetSessionAsync(command.UserId, command.SessionId, ct);
        if (sessionResult.Status != PracticeQueryStatus.Success)
        {
            return Result.Failure(RealtimeErrors.SessionAccessDenied);
        }

        var messageId = Guid.NewGuid();
        var now = _clock.UtcNow;

        // Preferred MVP behavior: broadcast final transcript event only.
        // Persisted transcript writes remain through the Practice HTTP/CQRS flow.
        await _notifier.NotifyTranscriptFinalAsync(
            command.SessionId,
            null,
            messageId,
            command.SequenceNumber,
            "user",
            command.Content,
            now,
            ct);

        return Result.Success();
    }
}
