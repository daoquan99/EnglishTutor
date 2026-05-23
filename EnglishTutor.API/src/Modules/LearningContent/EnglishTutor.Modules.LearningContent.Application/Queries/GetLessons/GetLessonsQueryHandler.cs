using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.Mappers;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetLessons;

public sealed class GetLessonsQueryHandler(
    ILessonRepository lessonRepository,
    IUserLanguageSettingsReader languageSettingsReader)
    : IQueryHandler<GetLessonsQuery, IReadOnlyList<LessonListResponse>>
{
    public async Task<Result<IReadOnlyList<LessonListResponse>>> Handle(GetLessonsQuery request, CancellationToken cancellationToken)
    {
        var settings = await languageSettingsReader.GetByUserIdAsync(request.UserId, cancellationToken);
        var uiLanguageCode = settings?.UiLanguageCode ?? "en";
        var targetLanguageCode = request.TargetLanguageCode ?? settings?.TargetLanguageCode;
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var lessons = await lessonRepository.ListPublishedAsync(
            page,
            pageSize,
            request.Level,
            request.Topic,
            request.Skill,
            targetLanguageCode,
            cancellationToken);

        return Result.Success<IReadOnlyList<LessonListResponse>>(lessons.Select(lesson => lesson.ToListResponse(uiLanguageCode)).ToArray());
    }
}
