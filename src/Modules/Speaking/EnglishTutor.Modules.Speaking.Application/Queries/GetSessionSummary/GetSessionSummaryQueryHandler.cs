using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Commands.CompleteSession;
using EnglishTutor.Modules.Speaking.Application.DTOs;
using EnglishTutor.Modules.Speaking.Application.Errors;

namespace EnglishTutor.Modules.Speaking.Application.Queries.GetSessionSummary;

public sealed class GetSessionSummaryQueryHandler(ISpeakingSessionSummaryRepository speakingSessionSummaryRepository)
    : IQueryHandler<GetSessionSummaryQuery, SpeakingSessionSummaryResponse>
{
    public async Task<Result<SpeakingSessionSummaryResponse>> Handle(GetSessionSummaryQuery request, CancellationToken cancellationToken)
    {
        var summary = await speakingSessionSummaryRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
        if (summary is null || summary.UserId != request.UserId)
        {
            return Result.Failure<SpeakingSessionSummaryResponse>(SpeakingErrors.SessionNotFound(request.SessionId));
        }

        return CompleteSpeakingSessionCommandHandler.ToResponse(summary);
    }
}
