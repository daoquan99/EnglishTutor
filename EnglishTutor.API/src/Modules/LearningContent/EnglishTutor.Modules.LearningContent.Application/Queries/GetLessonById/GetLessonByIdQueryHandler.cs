using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.Errors;
using EnglishTutor.Modules.LearningContent.Application.Shared.Mappers;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetLessonById;

public sealed class GetLessonByIdQueryHandler(
    ILessonRepository lessonRepository,
    IUserLanguageSettingsReader languageSettingsReader)
    : IQueryHandler<GetLessonByIdQuery, LessonDetailResponse>
{
    public async Task<Result<LessonDetailResponse>> Handle(GetLessonByIdQuery request, CancellationToken cancellationToken)
    {
        var lesson = await lessonRepository.GetByIdWithDetailsAsync(request.LessonId, cancellationToken);
        if (lesson is null)
        {
            return Result.Failure<LessonDetailResponse>(LearningContentErrors.LessonNotFound(request.LessonId));
        }

        if (!lesson.IsPublished)
        {
            return Result.Failure<LessonDetailResponse>(LearningContentErrors.ContentNotPublished);
        }

        var settings = await languageSettingsReader.GetByUserIdAsync(request.UserId, cancellationToken);
        return lesson.ToDetailResponse(settings?.UiLanguageCode ?? "en");
    }
}
