using System;
using System.Collections.Generic;
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

namespace EnglishTutor.Practice.Application.Sessions.Queries.ListPracticeSessions;

public sealed class ListPracticeSessionsHandler : IQueryHandler<ListPracticeSessionsQuery, PracticeSessionPage>
{
    private readonly IPracticeSessionRepository _sessions;
    private readonly IDateTimeProvider _clock;

    public ListPracticeSessionsHandler(
        IPracticeSessionRepository sessions,
        IDateTimeProvider clock)
    {
        _sessions = sessions;
        _clock = clock;
    }

    public async Task<Result<PracticeSessionPage>> Handle(ListPracticeSessionsQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var total = await _sessions.GetCountByUserIdAsync(request.UserId, ct);
        var sessions = await _sessions.GetByUserIdAsync(request.UserId, (page - 1) * pageSize, pageSize, ct);

        var now = _clock.UtcNow;
        var items = new List<PracticeSessionSummary>();
        foreach (var s in sessions)
        {
            var status = s.Status;
            var endedAt = s.EndedAtUtc;

            if (PracticeSessionExpiryPolicy.IsExpired(s, now))
            {
                status = PracticeSessionStatus.Expired;
                endedAt = s.ExpiresAtUtc;
            }

            items.Add(new PracticeSessionSummary(
                s.Id,
                s.UserId,
                status.ToString(),
                s.ScenarioSnapshot.ScenarioId,
                s.ScenarioSnapshot.TopicCode,
                s.ScenarioSnapshot.ModeCode,
                s.ScenarioSnapshot.Title,
                s.StartedAtUtc,
                endedAt,
                s.ExpiresAtUtc,
                s.TranscriptMessages.Count,
                s.LanguageSnapshot.LanguagePairId,
                s.LanguageSnapshot.NativeLanguageCode,
                s.LanguageSnapshot.TargetLanguageCode,
                s.LanguageSnapshot.ExplanationLanguageCode));
        }

        return Result.Success(new PracticeSessionPage(items, page, pageSize, total));
    }
}
