using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Mistakes.Infrastructure.Persistence.Repositories;

public sealed class MistakeRepository(MistakesDbContext dbContext) : IMistakeRepository
{
    public Task AddAsync(Mistake mistake, CancellationToken cancellationToken)
    {
        dbContext.Mistakes.Add(mistake);
        return Task.CompletedTask;
    }

    public Task<Mistake?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.Mistakes.SingleOrDefaultAsync(mistake => mistake.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Mistake>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        await dbContext.Mistakes
            .Where(mistake => mistake.UserId == userId)
            .OrderByDescending(mistake => mistake.CreatedAtUtc)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Mistake>> GetDueAsync(Guid userId, DateTime nowUtc, CancellationToken cancellationToken) =>
        await dbContext.Mistakes
            .Where(mistake => mistake.UserId == userId && mistake.NextReviewAtUtc <= nowUtc)
            .OrderBy(mistake => mistake.NextReviewAtUtc)
            .ToListAsync(cancellationToken);
}
