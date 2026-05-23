using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Commands.StartSession;
using EnglishTutor.Modules.Speaking.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Queries.GetSessions;

public sealed class GetSessionsQueryHandler(ISpeakingSessionRepository speakingSessionRepository)
    : IQueryHandler<GetSessionsQuery, IReadOnlyList<SpeakingSessionResponse>>
{
    private const int MaxPageSize = 100;

    public async Task<Result<IReadOnlyList<SpeakingSessionResponse>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, MaxPageSize);
        var skip = (page - 1) * pageSize;
        var sessions = await speakingSessionRepository.GetByUserIdAsync(request.UserId, skip, pageSize, cancellationToken);
        return sessions.Select(StartSpeakingSessionCommandHandler.ToResponse).ToList();
    }
}

