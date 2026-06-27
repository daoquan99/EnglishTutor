using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Realtime.Application.Abstractions;
using EnglishTutor.Realtime.Application.Errors;

namespace EnglishTutor.Realtime.Application.Transcript.Commands.PublishUserTranscriptPartial;

internal sealed class PublishUserTranscriptPartialCommandHandler : ICommandHandler<PublishUserTranscriptPartialCommand>
{
    private readonly IPracticeModule _practice;
    private readonly IRealtimeNotifier _notifier;

    public PublishUserTranscriptPartialCommandHandler(
        IPracticeModule practice,
        IRealtimeNotifier notifier)
    {
        _practice = practice ?? throw new ArgumentNullException(nameof(practice));
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
    }

    public async Task<Result> Handle(PublishUserTranscriptPartialCommand command, CancellationToken ct)
    {
        var sessionResult = await _practice.GetSessionAsync(command.UserId, command.SessionId, ct);
        if (sessionResult.Status != PracticeQueryStatus.Success)
        {
            return Result.Failure(RealtimeErrors.SessionAccessDenied);
        }

        await _notifier.NotifyTranscriptPartialAsync(
            command.SessionId,
            null,
            command.SequenceNumber,
            "user",
            command.Content,
            ct);

        return Result.Success();
    }
}
