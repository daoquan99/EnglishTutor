using EnglishTutor.Modules.Mistakes.Domain.Entities;

namespace EnglishTutor.Modules.Mistakes.Application.Abstractions;

public interface IMistakeRepository
{
    Task AddAsync(Mistake mistake, CancellationToken cancellationToken);

    Task<Mistake?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Mistake>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Mistake>> GetDueAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken);
}
