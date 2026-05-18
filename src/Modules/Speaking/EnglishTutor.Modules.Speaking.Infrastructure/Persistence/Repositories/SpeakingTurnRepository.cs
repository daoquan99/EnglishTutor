using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Speaking.Infrastructure.Persistence.Repositories;

public sealed class SpeakingTurnRepository(SpeakingDbContext dbContext) : ISpeakingTurnRepository
{
    public Task AddResultAsync(SpeakingTurnResult result, CancellationToken cancellationToken)
    {
        dbContext.SpeakingTurnResults.Add(result);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<SpeakingTurnResult>> GetResultsBySessionIdAsync(Guid speakingSessionId, CancellationToken cancellationToken)
    {
        var turnIds = await dbContext.SpeakingTurns
            .Where(turn => turn.SpeakingSessionId == speakingSessionId)
            .Select(turn => turn.Id)
            .ToListAsync(cancellationToken);

        return await dbContext.SpeakingTurnResults
            .Where(result => turnIds.Contains(result.SpeakingTurnId))
            .ToListAsync(cancellationToken);
    }
}
