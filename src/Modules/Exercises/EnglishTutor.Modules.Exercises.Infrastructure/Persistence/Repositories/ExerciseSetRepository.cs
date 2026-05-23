using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Exercises.Infrastructure.Persistence.Repositories;

public sealed class ExerciseSetRepository(ExercisesDbContext dbContext) : IExerciseSetRepository
{
    public Task<ExerciseSet?> GetByIdWithQuestionsAsync(Guid exerciseSetId, CancellationToken cancellationToken) =>
        dbContext.ExerciseSets
            .Include(exerciseSet => exerciseSet.Questions)
                .ThenInclude(question => question.Options)
            .SingleOrDefaultAsync(exerciseSet => exerciseSet.Id == exerciseSetId, cancellationToken);

    public async Task<IReadOnlyList<ExerciseSet>> ListPublishedAsync(
        int page,
        int pageSize,
        string? level,
        string? type,
        string? topic,
        string? skill,
        string? targetLanguageCode,
        CancellationToken cancellationToken)
    {
        var query = dbContext.ExerciseSets
            .Where(exerciseSet => exerciseSet.IsPublished);

        if (!string.IsNullOrWhiteSpace(targetLanguageCode))
        {
            var normalized = targetLanguageCode.Trim().ToLowerInvariant();
            query = query.Where(exerciseSet => exerciseSet.TargetLanguageCode == normalized);
        }

        if (Enum.TryParse<LanguageLevel>(level, true, out var parsedLevel))
        {
            query = query.Where(exerciseSet => exerciseSet.Level == parsedLevel);
        }

        if (Enum.TryParse<ExerciseType>(type, true, out var parsedType))
        {
            query = query.Where(exerciseSet => exerciseSet.ExerciseType == parsedType);
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            var normalizedTopic = topic.Trim().ToLowerInvariant();
            query = query.Where(exerciseSet => exerciseSet.Topic.ToLower() == normalizedTopic);
        }

        if (Enum.TryParse<LearningSkill>(skill, true, out var parsedSkill))
        {
            query = query.Where(exerciseSet => exerciseSet.Skill == parsedSkill);
        }

        return await query
            .OrderBy(exerciseSet => exerciseSet.Level)
            .ThenBy(exerciseSet => exerciseSet.ExerciseType)
            .ThenBy(exerciseSet => exerciseSet.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ExerciseSet exerciseSet, CancellationToken cancellationToken) =>
        await dbContext.ExerciseSets.AddAsync(exerciseSet, cancellationToken);
}
