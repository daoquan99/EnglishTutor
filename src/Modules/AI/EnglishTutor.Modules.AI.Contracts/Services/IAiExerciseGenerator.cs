using EnglishTutor.Modules.AI.Contracts.DTOs;

namespace EnglishTutor.Modules.AI.Contracts.Services;

public interface IAiExerciseGenerator
{
    Task<ExerciseGenerationResponse> GenerateAsync(ExerciseGenerationRequest request, CancellationToken cancellationToken);
}
