using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Exercises.Infrastructure.Seed;

public sealed class ExerciseDataSeeder(
    ExercisesDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var exerciseSet in ExerciseSeedData.CreateDefaultSets(dateTimeProvider.UtcNow))
        {
            var exists = await dbContext.ExerciseSets.AnyAsync(
                candidate => candidate.Title == exerciseSet.Title,
                cancellationToken);

            if (!exists)
            {
                dbContext.ExerciseSets.Add(exerciseSet);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
