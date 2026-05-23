using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.AI.Infrastructure.Persistence.Repositories;

public sealed class AiProviderRepository(AiDbContext dbContext) : IAiProviderRepository
{
    public async Task<IReadOnlyList<AiProvider>> ListAsync(CancellationToken cancellationToken) =>
        await dbContext.AiProviders
            .Include(provider => provider.Models)
            .OrderBy(provider => provider.ProviderName)
            .ToListAsync(cancellationToken);

    public Task<AiProvider?> GetByNameAsync(string providerName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            return Task.FromResult<AiProvider?>(null);
        }

        var normalizedName = providerName.Trim().ToLowerInvariant();
        return dbContext.AiProviders
            .Include(provider => provider.Models)
            .SingleOrDefaultAsync(provider => provider.ProviderName == normalizedName, cancellationToken);
    }

    public async Task AddAsync(AiProvider provider, CancellationToken cancellationToken) =>
        await dbContext.AiProviders.AddAsync(provider, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        dbContext.SaveChangesAsync(cancellationToken);
}
