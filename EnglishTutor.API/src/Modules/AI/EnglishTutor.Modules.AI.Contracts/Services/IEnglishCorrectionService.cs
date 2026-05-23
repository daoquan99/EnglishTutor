using EnglishTutor.Modules.AI.Contracts.DTOs;

namespace EnglishTutor.Modules.AI.Contracts.Services;

public interface IEnglishCorrectionService
{
    Task<CorrectionResponse> CorrectSentenceAsync(CorrectionRequest request, CancellationToken cancellationToken);
}
