using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class AiCostEstimation : Entity<Guid>
{
    public AiModelType ModelType { get; private set; }
    public int PromptTokens { get; private set; }
    public int CompletionTokens { get; private set; }
    public decimal EstimatedCostUsd { get; private set; }
    public DateTime EstimatedAtUtc { get; private set; }

    private AiCostEstimation() { }

    public static AiCostEstimation Create(
        AiModelType modelType,
        int promptTokens,
        int completionTokens,
        decimal estimatedCostUsd)
    {
        if (promptTokens < 0 || completionTokens < 0)
        {
            throw new DomainException("Token counts cannot be negative.");
        }

        if (estimatedCostUsd < 0)
        {
            throw new DomainException("Estimated cost cannot be negative.");
        }

        return new AiCostEstimation
        {
            Id = Guid.NewGuid(),
            ModelType = modelType,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            EstimatedCostUsd = estimatedCostUsd,
            EstimatedAtUtc = DateTime.UtcNow
        };
    }
}
