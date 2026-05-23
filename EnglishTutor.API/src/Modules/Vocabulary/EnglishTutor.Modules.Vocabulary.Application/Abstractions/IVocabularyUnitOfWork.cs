namespace EnglishTutor.Modules.Vocabulary.Application.Abstractions;

public interface IVocabularyUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
