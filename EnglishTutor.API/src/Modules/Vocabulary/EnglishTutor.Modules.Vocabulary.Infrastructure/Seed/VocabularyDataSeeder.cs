using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Seed;

public sealed class VocabularyDataSeeder(
    VocabularyDbContext dbContext,
    IDateTimeProvider dateTimeProvider)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var defaultItem in VocabularySeedData.CreateDefaultEnglishItems(dateTimeProvider.UtcNow))
        {
            var exists = await dbContext.VocabularyItems.AnyAsync(
                item => item.TargetLanguageCode == defaultItem.TargetLanguageCode &&
                    item.Word == defaultItem.Word &&
                    item.PartOfSpeech == defaultItem.PartOfSpeech,
                cancellationToken);

            if (!exists)
            {
                dbContext.VocabularyItems.Add(defaultItem);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
