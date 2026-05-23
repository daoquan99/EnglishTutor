using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;
using EnglishTutor.Modules.Assessments.Application.Shared.Errors;
using EnglishTutor.Modules.Assessments.Application.Shared.Mappers;

namespace EnglishTutor.Modules.Assessments.Application.Queries.GetAttempt;

public sealed class GetAttemptQueryHandler(
    IAssessmentAttemptRepository attemptRepository,
    IAssessmentDefinitionRepository definitionRepository)
    : IQueryHandler<GetAttemptQuery, AttemptDetailResponse>
{
    public async Task<Result<AttemptDetailResponse>> Handle(GetAttemptQuery request, CancellationToken cancellationToken)
    {
        var attempt = await attemptRepository.GetByIdWithAnswersAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.AttemptNotFound(request.AttemptId));
        }

        if (attempt.UserId != request.UserId)
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.AttemptNotOwned);
        }

        var definition = await definitionRepository.GetByIdWithDetailsAsync(attempt.AssessmentDefinitionId, cancellationToken);
        return definition is null
            ? Result.Failure<AttemptDetailResponse>(AssessmentErrors.DefinitionNotFound)
            : attempt.ToDetailResponse(definition);
    }
}
