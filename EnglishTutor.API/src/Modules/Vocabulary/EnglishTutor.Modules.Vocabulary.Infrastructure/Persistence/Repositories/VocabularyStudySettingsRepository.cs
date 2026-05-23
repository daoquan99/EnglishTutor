using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Repositories;

public sealed class VocabularyStudySettingsRepository(VocabularyDbContext dbContext) : IVocabularyStudySettingsRepository
{
    public Task<VocabularyStudySettings?> GetAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken)
    {
        var languageCode = LanguageCode.Create(targetLanguageCode);
        return dbContext.VocabularyStudySettings.SingleOrDefaultAsync(
            s => s.UserId == userId && s.TargetLanguageCode == languageCode,
            cancellationToken);
    }

    public Task AddAsync(VocabularyStudySettings settings, CancellationToken cancellationToken)
    {
        dbContext.VocabularyStudySettings.Add(settings);
        return Task.CompletedTask;
    }
}
