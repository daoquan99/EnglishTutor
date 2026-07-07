namespace EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.Repositories;

public interface ILanguageDefinitionRepository
{
    Task<LanguageDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LanguageDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LanguageDefinition>> ListAsync(
        bool? active,
        CancellationToken cancellationToken = default);
    Task AddAsync(LanguageDefinition language, CancellationToken cancellationToken = default);
    void Update(LanguageDefinition language);
}
