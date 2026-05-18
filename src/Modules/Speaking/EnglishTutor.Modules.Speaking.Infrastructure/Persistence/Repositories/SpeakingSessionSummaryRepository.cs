using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Speaking.Infrastructure.Persistence.Repositories;

public sealed class SpeakingSessionSummaryRepository(SpeakingDbContext dbContext) : ISpeakingSessionSummaryRepository
{
    public Task AddAsync(SpeakingSessionSummary summary, CancellationToken cancellationToken)
    {
        dbContext.SpeakingSessionSummaries.Add(summary);
        return Task.CompletedTask;
    }

    public Task<SpeakingSessionSummary?> GetBySessionIdAsync(Guid speakingSessionId, CancellationToken cancellationToken) =>
        dbContext.SpeakingSessionSummaries.SingleOrDefaultAsync(summary => summary.SpeakingSessionId == speakingSessionId, cancellationToken);
}
