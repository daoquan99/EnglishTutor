using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Repositories;

public sealed class UserVocabularyMasteryRepository(VocabularyDbContext dbContext) : IUserVocabularyMasteryRepository
{
    public Task<UserVocabularyMastery?> GetAsync(Guid userId, Guid vocabularyItemId, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return dbContext.UserVocabularyMasteries.SingleOrDefaultAsync(
            mastery => mastery.UserId == userId &&
                       mastery.VocabularyItemId == vocabularyItemId &&
                       mastery.TargetLanguageCode == languageCode,
            cancellationToken);
    }

    public async Task<IReadOnlyList<UserVocabularyMastery>> GetDueAsync(
        Guid userId,
        string targetLanguageCode,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return await dbContext.UserVocabularyMasteries
            .Where(mastery => mastery.UserId == userId &&
                              mastery.TargetLanguageCode == languageCode &&
                              mastery.NextReviewAtUtc <= nowUtc)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(UserVocabularyMastery mastery, CancellationToken cancellationToken)
    {
        dbContext.UserVocabularyMasteries.Add(mastery);
        return Task.CompletedTask;
    }
}
