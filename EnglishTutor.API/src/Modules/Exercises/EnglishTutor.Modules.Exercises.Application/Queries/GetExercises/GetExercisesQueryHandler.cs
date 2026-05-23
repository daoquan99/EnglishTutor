using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Shared.Mappers;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Exercises.Application.Queries.GetExercises;

public sealed class GetExercisesQueryHandler(
    IExerciseSetRepository exerciseSetRepository,
    IUserLanguageSettingsReader languageSettingsReader)
    : IQueryHandler<GetExercisesQuery, IReadOnlyList<ExerciseListResponse>>
{
    public async Task<Result<IReadOnlyList<ExerciseListResponse>>> Handle(GetExercisesQuery request, CancellationToken cancellationToken)
    {
        var settings = await languageSettingsReader.GetByUserIdAsync(request.UserId, cancellationToken);
        var targetLanguageCode = request.TargetLanguageCode ?? settings?.TargetLanguageCode;
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var exercises = await exerciseSetRepository.ListPublishedAsync(
            page,
            pageSize,
            request.Level,
            request.Type,
            request.Topic,
            request.Skill,
            targetLanguageCode,
            cancellationToken);

        return Result.Success<IReadOnlyList<ExerciseListResponse>>(exercises.Select(exercise => exercise.ToListResponse()).ToArray());
    }
}
