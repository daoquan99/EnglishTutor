using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Application.Sessions.Abstractions;
using EnglishTutor.Practice.Application.Sessions.Policies;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

namespace EnglishTutor.Practice.Application.Sessions.Queries.GetPracticeSession;

public sealed class GetPracticeSessionHandler : IQueryHandler<GetPracticeSessionQuery, GetSessionResult>
{
    private readonly IPracticeSessionRepository _sessions;
    private readonly IDateTimeProvider _clock;

    public GetPracticeSessionHandler(
        IPracticeSessionRepository sessions,
        IDateTimeProvider clock)
    {
        _sessions = sessions;
        _clock = clock;
    }

    public async Task<Result<GetSessionResult>> Handle(GetPracticeSessionQuery request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
        {
            return Result.Success(new GetSessionResult(PracticeQueryStatus.NotFound, null));
        }

        if (session.UserId != request.UserId)
        {
            return Result.Success(new GetSessionResult(PracticeQueryStatus.Forbidden, null));
        }

        var now = _clock.UtcNow;
        var status = session.Status;
        var endReason = session.EndReason;
        var endedAt = session.EndedAtUtc;

        if (PracticeSessionExpiryPolicy.IsExpired(session, now))
        {
            status = PracticeSessionStatus.Expired;
            endedAt = session.ExpiresAtUtc;
        }

        var summary = new PracticeSessionSummary(
            session.Id,
            session.UserId,
            status.ToString(),
            session.ScenarioSnapshot.ScenarioId,
            session.ScenarioSnapshot.TopicCode,
            session.ScenarioSnapshot.ModeCode,
            session.ScenarioSnapshot.Title,
            session.StartedAtUtc,
            endedAt,
            session.ExpiresAtUtc,
            session.TranscriptMessages.Count);

        return Result.Success(new GetSessionResult(PracticeQueryStatus.Success, summary));
    }
}
