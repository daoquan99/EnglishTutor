using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence;

public sealed class LearningLanguageCatalogSeeder
{
    private readonly LearningDbContext _dbContext;

    public LearningLanguageCatalogSeeder(LearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var definitions = new[]
        {
            LanguageDefinition.Create(Guid.Parse("9701aa72-3e41-4a40-8fe9-100000000001"), "vi", "Vietnamese", "Tiếng Việt", true, true, 10),
            LanguageDefinition.Create(Guid.Parse("9701aa72-3e41-4a40-8fe9-100000000002"), "en", "English", "English", true, true, 20),
            LanguageDefinition.Create(Guid.Parse("9701aa72-3e41-4a40-8fe9-100000000003"), "zh-CN", "Chinese (Simplified)", "简体中文", true, true, 30),
            LanguageDefinition.Create(Guid.Parse("9701aa72-3e41-4a40-8fe9-100000000004"), "ja", "Japanese", "日本語", true, true, 40)
        };

        var existingCodes = await _dbContext.LanguageDefinitions
            .IgnoreQueryFilters()
            .Select(item => item.Code)
            .ToListAsync(cancellationToken);

        foreach (var definition in definitions.Where(item => !existingCodes.Contains(item.Code)))
        {
            await _dbContext.LanguageDefinitions.AddAsync(definition, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
