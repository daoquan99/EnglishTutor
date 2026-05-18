using EnglishTutor.Modules.Vocabulary.Domain.Entities;

namespace EnglishTutor.Modules.Vocabulary.Application.Abstractions;

public interface IVocabularyItemRepository
{
    Task<VocabularyItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<VocabularyExample?> GetExampleByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<VocabularyItem>> GetByTargetLanguageAsync(string targetLanguageCode, CancellationToken cancellationToken);
}
