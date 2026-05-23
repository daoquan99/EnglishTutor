using System.Text.Json;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;
using EnglishTutor.Modules.Assessments.Application.Shared.Errors;

namespace EnglishTutor.Modules.Assessments.Application.Queries.GetAttemptResult;

public sealed class GetAttemptResultQueryHandler(IAssessmentAttemptRepository attemptRepository)
    : IQueryHandler<GetAttemptResultQuery, AttemptResultResponse>
{
    public async Task<Result<AttemptResultResponse>> Handle(GetAttemptResultQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attemptRepository.GetByIdWithAnswersAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<AttemptResultResponse>(AssessmentErrors.AttemptNotFound(request.AttemptId));
        }

        if (attempt.UserId != request.UserId)
        {
            return Result.Failure<AttemptResultResponse>(AssessmentErrors.AttemptNotOwned);
        }

        var result = await attemptRepository.GetResultByAttemptIdAsync(request.AttemptId, cancellationToken);
        if (result is null)
        {
            return Result.Failure<AttemptResultResponse>(AssessmentErrors.ResultNotFound);
        }

        var sectionScores = JsonSerializer.Deserialize<Dictionary<string, int>>(result.SectionScoresJson) ?? [];
        return new AttemptResultResponse(
            attempt.Id,
            attempt.Status.ToString(),
            result.TotalScore,
            result.IsPassed,
            sectionScores.Select(pair => new SectionScoreResponse(pair.Key, pair.Value)).ToArray(),
            result.GradedAtUtc);
    }
}
