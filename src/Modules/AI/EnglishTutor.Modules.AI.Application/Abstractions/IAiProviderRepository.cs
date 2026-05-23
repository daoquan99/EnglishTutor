using EnglishTutor.Modules.AI.Domain.Entities;

namespace EnglishTutor.Modules.AI.Application.Abstractions;

public interface IAiProviderRepository
{
    Task<IReadOnlyList<AiProvider>> ListAsync(CancellationToken cancellationToken);

    Task<AiProvider?> GetByNameAsync(string providerName, CancellationToken cancellationToken);

    Task AddAsync(AiProvider provider, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
