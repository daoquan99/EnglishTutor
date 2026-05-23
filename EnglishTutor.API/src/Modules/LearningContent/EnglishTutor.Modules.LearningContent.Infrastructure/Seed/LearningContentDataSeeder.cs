using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Seed;

public sealed class LearningContentDataSeeder(
    LearningContentDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = dateTimeProvider.UtcNow;
        foreach (var lesson in LearningContentSeedData.CreateLessons(utcNow))
        {
            if (!await dbContext.Lessons.AnyAsync(candidate => candidate.Title == lesson.Title, cancellationToken))
            {
                dbContext.Lessons.Add(lesson);
            }
        }

        foreach (var scenario in LearningContentSeedData.CreateScenarios(utcNow))
        {
            if (!await dbContext.ConversationScenarios.AnyAsync(candidate => candidate.Title == scenario.Title, cancellationToken))
            {
                dbContext.ConversationScenarios.Add(scenario);
            }
        }

        foreach (var pattern in LearningContentSeedData.CreateSentencePatterns(utcNow))
        {
            if (!await dbContext.SentencePatterns.AnyAsync(candidate => candidate.Pattern == pattern.Pattern, cancellationToken))
            {
                dbContext.SentencePatterns.Add(pattern);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
