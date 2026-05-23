using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Shared.Errors;
using EnglishTutor.Modules.Exercises.Application.Shared.Mappers;

namespace EnglishTutor.Modules.Exercises.Application.Queries.GetExerciseById;

public sealed class GetExerciseByIdQueryHandler(IExerciseSetRepository exerciseSetRepository)
    : IQueryHandler<GetExerciseByIdQuery, ExerciseDetailResponse>
{
    public async Task<Result<ExerciseDetailResponse>> Handle(GetExerciseByIdQuery request, CancellationToken cancellationToken)
    {
        var exercise = await exerciseSetRepository.GetByIdWithQuestionsAsync(request.ExerciseSetId, cancellationToken);
        if (exercise is null)
        {
            return Result.Failure<ExerciseDetailResponse>(ExerciseErrors.ExerciseSetNotFound(request.ExerciseSetId));
        }

        if (!exercise.IsPublished)
        {
            return Result.Failure<ExerciseDetailResponse>(ExerciseErrors.ContentNotPublished);
        }

        return exercise.ToDetailResponse();
    }
}
