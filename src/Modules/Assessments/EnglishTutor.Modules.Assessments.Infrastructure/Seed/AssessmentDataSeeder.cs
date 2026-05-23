using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Assessments.Infrastructure.Seed;

public sealed class AssessmentDataSeeder(
    AssessmentsDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var definition in AssessmentSeedData.CreateDefaultAssessments(dateTimeProvider.UtcNow))
        {
            var exists = await dbContext.AssessmentDefinitions.AnyAsync(
                candidate =>
                    candidate.AssessmentType == definition.AssessmentType &&
                    candidate.TargetLanguageCode == definition.TargetLanguageCode &&
                    candidate.ForLevel == definition.ForLevel,
                cancellationToken);

            if (!exists)
            {
                dbContext.AssessmentDefinitions.Add(definition);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
