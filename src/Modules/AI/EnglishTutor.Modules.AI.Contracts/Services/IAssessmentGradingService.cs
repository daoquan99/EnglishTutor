using EnglishTutor.Modules.AI.Contracts.DTOs;

namespace EnglishTutor.Modules.AI.Contracts.Services;

public interface IAssessmentGradingService
{
    Task<GradingResponse> GradeAsync(GradingRequest request, CancellationToken cancellationToken);
}
