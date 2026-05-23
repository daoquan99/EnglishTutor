namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetLessons;

public sealed record LessonListResponse(
    Guid Id,
    string TargetLanguageCode,
    string Level,
    string Topic,
    string Skill,
    string Title,
    string Description,
    int Order,
    int EstimatedMinutes);
