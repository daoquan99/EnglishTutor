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
}
