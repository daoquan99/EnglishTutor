using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Speaking.Infrastructure.Persistence.Repositories;

public sealed class SpeakingTurnRepository(SpeakingDbContext dbContext) : ISpeakingTurnRepository
{
    public async Task AddTurnAsync(SpeakingTurn turn, CancellationToken cancellationToken) =>
        await dbContext.SpeakingTurns.AddAsync(turn, cancellationToken);

    public Task AddResultAsync(SpeakingTurnResult result, CancellationToken cancellationToken)
    {
        dbContext.SpeakingTurnResults.Add(result);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<SpeakingTurnResult>> GetResultsBySessionIdAsync(Guid speakingSessionId, CancellationToken cancellationToken)
    {
        return await (
            from result in dbContext.SpeakingTurnResults
            join turn in dbContext.SpeakingTurns on result.SpeakingTurnId equals turn.Id
            where turn.SpeakingSessionId == speakingSessionId
            select result)
            .ToListAsync(cancellationToken);
    }
}
