using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Shared.DTOs;
using EnglishTutor.Modules.Exercises.Application.Shared.Errors;
using EnglishTutor.Modules.Exercises.Application.Shared.Mappers;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;

namespace EnglishTutor.Modules.Exercises.Application.Queries.GetAttemptResult;

public sealed class GetAttemptResultQueryHandler(
    IExerciseAttemptRepository exerciseAttemptRepository,
    IExerciseSetRepository exerciseSetRepository)
    : IQueryHandler<GetAttemptResultQuery, ExerciseResultResponse>
{
    public async Task<Result<ExerciseResultResponse>> Handle(GetAttemptResultQuery request, CancellationToken cancellationToken)
    {
        var attempt = await exerciseAttemptRepository.GetByIdWithAnswersAsync(request.AttemptId, cancellationToken);
        if (attempt is null)
        {
            return Result.Failure<ExerciseResultResponse>(ExerciseErrors.AttemptNotFound(request.AttemptId));
        }

        if (attempt.UserId != request.UserId)
        {
            return Result.Failure<ExerciseResultResponse>(ExerciseErrors.Forbidden);
        }

        if (attempt.Status != ExerciseAttemptStatus.Completed)
        {
            return Result.Failure<ExerciseResultResponse>(ExerciseErrors.ResultNotFound(request.AttemptId));
        }

        var exerciseSet = await exerciseSetRepository.GetByIdWithQuestionsAsync(attempt.ExerciseSetId, cancellationToken);
        if (exerciseSet is null)
        {
            return Result.Failure<ExerciseResultResponse>(ExerciseErrors.ExerciseSetNotFound(attempt.ExerciseSetId));
        }

        return attempt.ToResultResponse(exerciseSet);
    }
}
