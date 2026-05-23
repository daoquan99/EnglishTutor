using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Abstractions;

public interface IVocabularyStudySettingsRepository
{
    Task<VocabularyStudySettings?> GetAsync(Guid userId, string targetLanguageCode, CancellationToken cancellationToken);
    Task AddAsync(VocabularyStudySettings settings, CancellationToken cancellationToken);
}
