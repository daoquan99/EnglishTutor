using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;

namespace EnglishTutor.Practice.Application.Sessions.Queries.GetPracticeTranscript;

public sealed class GetPracticeTranscriptHandler : IQueryHandler<GetPracticeTranscriptQuery, GetTranscriptResult>
{
    private readonly IPracticeSessionRepository _sessions;

    public GetPracticeTranscriptHandler(IPracticeSessionRepository sessions)
    {
        _sessions = sessions;
    }

    public async Task<Result<GetTranscriptResult>> Handle(GetPracticeTranscriptQuery request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
        {
            return Result.Success(new GetTranscriptResult(PracticeQueryStatus.NotFound, []));
        }

        if (session.UserId != request.UserId)
        {
            return Result.Success(new GetTranscriptResult(PracticeQueryStatus.Forbidden, []));
        }

        var messages = session.TranscriptMessages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new TranscriptMessageDto(m.Id, m.SequenceNumber, m.Role, m.Content, m.CreatedAtUtc))
            .ToList();

        return Result.Success(new GetTranscriptResult(PracticeQueryStatus.Success, messages));
    }
}
