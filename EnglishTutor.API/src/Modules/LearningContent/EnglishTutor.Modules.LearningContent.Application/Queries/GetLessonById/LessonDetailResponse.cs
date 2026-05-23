namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetLessonById;

public sealed record LessonDetailResponse(
    Guid Id,
    string TargetLanguageCode,
    string Level,
    string Topic,
    string Skill,
    string Title,
    string Description,
    int Order,
    int EstimatedMinutes,
    IReadOnlyList<LessonSectionResponse> Sections);

public sealed record LessonSectionResponse(
    Guid Id,
    string Title,
    string Content,
    int Order,
    string SectionType);
