using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Repositories;

internal sealed class LanguageDefinitionRepository : ILanguageDefinitionRepository
{
    private readonly LearningDbContext _dbContext;

    public LanguageDefinitionRepository(LearningDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LanguageDefinition?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _dbContext.LanguageDefinitions.FirstOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);

    public Task<LanguageDefinition?> GetByCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        _dbContext.LanguageDefinitions.FirstOrDefaultAsync(
            item => item.Code == code,
            cancellationToken);

    public async Task<IReadOnlyList<LanguageDefinition>> ListAsync(
        bool? active,
        CancellationToken cancellationToken = default) =>
        await _dbContext.LanguageDefinitions
            .AsNoTracking()
            .Where(item => active == null || item.IsActive == active)
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.EnglishName)
            .ThenBy(item => item.Id)
            .ToListAsync(cancellationToken);

    public Task AddAsync(
        LanguageDefinition language,
        CancellationToken cancellationToken = default) =>
        _dbContext.LanguageDefinitions.AddAsync(language, cancellationToken).AsTask();

    public void Update(LanguageDefinition language) =>
        _dbContext.LanguageDefinitions.Update(language);
}
