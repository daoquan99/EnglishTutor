using EnglishTutor.Modules.LearningContent.Application.Queries.GetConversationById;
using EnglishTutor.Modules.LearningContent.Application.Queries.GetConversations;
using EnglishTutor.Modules.LearningContent.Application.Queries.GetLessonById;
using EnglishTutor.Modules.LearningContent.Application.Queries.GetLessons;
using EnglishTutor.Modules.LearningContent.Application.Shared.DTOs;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario;
using EnglishTutor.Modules.LearningContent.Domain.Lesson;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;

namespace EnglishTutor.Modules.LearningContent.Application.Shared.Mappers;

internal static class LearningContentMappers
{
    public static LessonListResponse ToListResponse(this Lesson lesson, string uiLanguageCode)
    {
        var translation = lesson.Translations.FirstOrDefault(candidate => candidate.LanguageCode == uiLanguageCode);
        return new LessonListResponse(
            lesson.Id,
            lesson.TargetLanguageCode,
            lesson.Level.ToString(),
            lesson.Topic,
            lesson.Skill.ToString(),
            translation?.Title ?? lesson.Title,
            translation?.Description ?? lesson.Description,
            lesson.Order,
            lesson.EstimatedMinutes);
    }

    public static LessonDetailResponse ToDetailResponse(this Lesson lesson, string uiLanguageCode)
    {
        var translation = lesson.Translations.FirstOrDefault(candidate => candidate.LanguageCode == uiLanguageCode);
        return new LessonDetailResponse(
            lesson.Id,
            lesson.TargetLanguageCode,
            lesson.Level.ToString(),
            lesson.Topic,
            lesson.Skill.ToString(),
            translation?.Title ?? lesson.Title,
            translation?.Description ?? lesson.Description,
            lesson.Order,
            lesson.EstimatedMinutes,
            lesson.Sections
                .OrderBy(section => section.Order)
                .Select(section =>
                {
                    var sectionTranslation = section.Translations.FirstOrDefault(candidate => candidate.LanguageCode == uiLanguageCode);
                    return new LessonSectionResponse(
                        section.Id,
                        sectionTranslation?.Title ?? section.Title,
                        sectionTranslation?.Content ?? section.Content,
                        section.Order,
                        section.SectionType);
                })
                .ToArray());
    }

    public static ConversationListResponse ToListResponse(this ConversationScenario scenario, string uiLanguageCode)
    {
        var translation = scenario.Translations.FirstOrDefault(candidate => candidate.LanguageCode == uiLanguageCode);
        return new ConversationListResponse(
            scenario.Id,
            scenario.TargetLanguageCode,
            scenario.Level.ToString(),
            translation?.Title ?? scenario.Title,
            translation?.Description ?? scenario.Description,
            translation?.Setting ?? scenario.Setting,
            scenario.Difficulty,
            scenario.EstimatedMinutes);
    }

    public static ConversationDetailResponse ToDetailResponse(this ConversationScenario scenario, string uiLanguageCode)
    {
        var translation = scenario.Translations.FirstOrDefault(candidate => candidate.LanguageCode == uiLanguageCode);
        return new ConversationDetailResponse(
            scenario.Id,
            scenario.TargetLanguageCode,
            scenario.Level.ToString(),
            translation?.Title ?? scenario.Title,
            translation?.Description ?? scenario.Description,
            translation?.Setting ?? scenario.Setting,
            scenario.Difficulty,
            scenario.EstimatedMinutes,
            scenario.Lines
                .OrderBy(line => line.Order)
                .Select(line =>
                {
                    var lineTranslation = line.Translations.FirstOrDefault(candidate => candidate.LanguageCode == uiLanguageCode);
                    return new ConversationLineResponse(
                        line.Id,
                        line.Order,
                        line.Speaker.ToString(),
                        lineTranslation?.Text ?? line.Text,
                        lineTranslation?.ExpectedResponseHint ?? line.ExpectedResponseHint,
                        line.AudioUrl,
                        line.Notes);
                })
                .ToArray());
    }

    public static LearningPathCardResponse ToResponse(this UserLearningPathCard card) =>
        new(
            card.Id,
            card.TargetLanguageCode,
            card.ContentType.ToString(),
            card.ContentId,
            card.Title,
            card.Level,
            card.Skill,
            card.Status.ToString(),
            card.Order,
            card.CompletedAtUtc,
            card.LastUpdatedAtUtc);
}
