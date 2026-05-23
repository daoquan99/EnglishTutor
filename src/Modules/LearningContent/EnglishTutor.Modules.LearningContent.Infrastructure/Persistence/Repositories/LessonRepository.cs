using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Domain.Lesson;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Persistence.Repositories;

public sealed class LessonRepository(LearningContentDbContext dbContext) : ILessonRepository
{
    public Task<Lesson?> GetByIdWithDetailsAsync(Guid lessonId, CancellationToken cancellationToken) =>
        dbContext.Lessons
            .Include(lesson => lesson.Translations)
            .Include(lesson => lesson.Sections)
                .ThenInclude(section => section.Translations)
            .SingleOrDefaultAsync(lesson => lesson.Id == lessonId, cancellationToken);

    public async Task<IReadOnlyList<Lesson>> ListPublishedAsync(
        int page,
        int pageSize,
        string? level,
        string? topic,
        string? skill,
        string? targetLanguageCode,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Lessons
            .Include(lesson => lesson.Translations)
            .Where(lesson => lesson.IsPublished);

        if (!string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            var normalized = targetLanguageCode.Trim().ToLowerInvariant();
            query = query.Where(lesson => lesson.TargetLanguageCode == normalized);
        }

        if (Enum.TryParse<LanguageLevel>(level, true, out var parsedLevel))
        {
            query = query.Where(lesson => lesson.Level == parsedLevel);
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            var normalizedTopic = topic.Trim().ToLowerInvariant();
            query = query.Where(lesson => lesson.Topic.ToLower() == normalizedTopic);
        }

        if (Enum.TryParse<LearningSkill>(skill, true, out var parsedSkill))
        {
            query = query.Where(lesson => lesson.Skill == parsedSkill);
        }

        return await query
            .OrderBy(lesson => lesson.Order)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }
}
