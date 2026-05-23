using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetLessons;

public sealed record GetLessonsQuery(
    Guid UserId,
    int Page,
    int PageSize,
    string? Level,
    string? Topic,
    string? Skill,
    string? TargetLanguageCode) : IQuery<IReadOnlyList<LessonListResponse>>;
