using EnglishTutor.Modules.Speaking.Domain.Entities;

namespace EnglishTutor.Modules.Speaking.Application.Abstractions;

public interface ISpeakingTurnRepository
{
    Task AddTurnAsync(SpeakingTurn turn, CancellationToken cancellationToken);

    Task AddResultAsync(SpeakingTurnResult result, CancellationToken cancellationToken);

    Task<IReadOnlyList<SpeakingTurnResult>> GetResultsBySessionIdAsync(Guid speakingSessionId, CancellationToken cancellationToken);
}
