using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Speaking.Infrastructure.Persistence.Repositories;

public sealed class SpeakingSessionRepository(SpeakingDbContext dbContext) : ISpeakingSessionRepository
{
    public Task AddAsync(SpeakingSession session, CancellationToken cancellationToken)
    {
        dbContext.SpeakingSessions.Add(session);
        return Task.CompletedTask;
    }

    public Task<SpeakingSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        dbContext.SpeakingSessions
            .Include(session => session.Turns)
            .SingleOrDefaultAsync(session => session.Id == id, cancellationToken);

    public async Task<IReadOnlyList<SpeakingSession>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken) =>
        await dbContext.SpeakingSessions
            .Where(session => session.UserId == userId)
            .OrderByDescending(session => session.StartedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
}
