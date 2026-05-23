using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Repositories;

public sealed class VocabularyItemRepository(VocabularyDbContext dbContext) : IVocabularyItemRepository
{
    public Task<VocabularyItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.VocabularyItems
            .Include(item => item.Translations)
            .Include(item => item.Examples)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

    public Task<VocabularyExample?> GetExampleByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.VocabularyExamples.SingleOrDefaultAsync(example => example.Id == id, cancellationToken);

    public async Task<IReadOnlyList<VocabularyItem>> GetByTargetLanguageAsync(string targetLanguageCode, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return await dbContext.VocabularyItems
            .Where(item => item.TargetLanguageCode == languageCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VocabularyItem>> GetByIdsAsync(
        IReadOnlyCollection<Guid> itemIds,
        CancellationToken cancellationToken) =>
        itemIds.Count == 0
            ? []
            : await dbContext.VocabularyItems
                .Where(item => itemIds.Contains(item.Id))
                .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<VocabularyItem>> GetNewItemsAsync(
        Guid userId,
        string targetLanguageCode,
        int take,
        CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return await dbContext.VocabularyItems
            .Where(item =>
                item.TargetLanguageCode == languageCode &&
                !dbContext.UserVocabularyMasteries.Any(mastery =>
                    mastery.UserId == userId &&
                    mastery.TargetLanguageCode == languageCode &&
                    mastery.VocabularyItemId == item.Id))
            .OrderBy(item => item.Word)
            .Take(Math.Max(0, take))
            .ToListAsync(cancellationToken);
    }
}
