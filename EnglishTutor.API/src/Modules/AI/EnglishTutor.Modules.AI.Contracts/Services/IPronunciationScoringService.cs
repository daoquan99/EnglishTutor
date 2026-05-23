using EnglishTutor.Modules.AI.Contracts.DTOs;

namespace EnglishTutor.Modules.AI.Contracts.Services;

public interface IPronunciationScoringService
{
    Task<PronunciationScoringResponse> ScoreAsync(PronunciationScoringRequest request, CancellationToken cancellationToken);
}
