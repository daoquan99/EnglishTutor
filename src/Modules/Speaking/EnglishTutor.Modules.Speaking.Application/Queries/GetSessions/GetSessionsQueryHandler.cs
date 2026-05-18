using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Commands.StartSession;
using EnglishTutor.Modules.Speaking.Application.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Queries.GetSessions;

public sealed class GetSessionsQueryHandler(ISpeakingSessionRepository speakingSessionRepository)
    : IQueryHandler<GetSessionsQuery, IReadOnlyList<SpeakingSessionResponse>>
{
    public async Task<Result<IReadOnlyList<SpeakingSessionResponse>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await speakingSessionRepository.GetByUserIdAsync(request.UserId, cancellationToken);
        return sessions.Select(StartSpeakingSessionCommandHandler.ToResponse).ToList();
    }
}
