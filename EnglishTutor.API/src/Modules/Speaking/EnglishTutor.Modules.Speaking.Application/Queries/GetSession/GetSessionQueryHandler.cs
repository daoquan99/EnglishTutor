using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Commands.StartSession;
using EnglishTutor.Modules.Speaking.Application.Shared.DTOs;
using EnglishTutor.Modules.Speaking.Application.Shared.Errors;

namespace EnglishTutor.Modules.Speaking.Application.Queries.GetSession;

public sealed class GetSessionQueryHandler(ISpeakingSessionRepository speakingSessionRepository)
    : IQueryHandler<GetSessionQuery, SpeakingSessionResponse>
{
    public async Task<Result<SpeakingSessionResponse>> Handle(GetSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await speakingSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.UserId != request.UserId)
        {
            return Result.Failure<SpeakingSessionResponse>(SpeakingErrors.SessionNotFound(request.SessionId));
        }

        return StartSpeakingSessionCommandHandler.ToResponse(session);
    }
}
