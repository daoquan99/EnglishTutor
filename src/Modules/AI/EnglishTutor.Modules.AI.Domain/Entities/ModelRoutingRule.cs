using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class ModelRoutingRule : Entity<Guid>
{
    public AiTaskType TaskType { get; private set; }
    public AiModelType PreferredModel { get; private set; }
    public AiModelType FallbackModel { get; private set; }
    public int MaxTokens { get; private set; }
    public decimal Temperature { get; private set; }
    public bool IsActive { get; private set; }

    private ModelRoutingRule() { }

    public static ModelRoutingRule Create(
        AiTaskType taskType,
        AiModelType preferredModel,
        AiModelType fallbackModel,
        int maxTokens,
        decimal temperature)
    {
        if (maxTokens <= 0)
        {
            throw new DomainException("Max tokens must be positive.");
        }

        if (temperature is < 0m or > 2m)
        {
            throw new DomainException("Temperature must be between 0 and 2.");
        }

        return new ModelRoutingRule
        {
            Id = Guid.NewGuid(),
            TaskType = taskType,
            PreferredModel = preferredModel,
            FallbackModel = fallbackModel,
            MaxTokens = maxTokens,
            Temperature = temperature,
            IsActive = true
        };
    }

    public void Update(AiModelType preferredModel, AiModelType fallbackModel, int maxTokens, decimal temperature)
    {
        if (maxTokens <= 0)
        {
            throw new DomainException("Max tokens must be positive.");
        }

        if (temperature is < 0m or > 2m)
        {
            throw new DomainException("Temperature must be between 0 and 2.");
        }

        PreferredModel = preferredModel;
        FallbackModel = fallbackModel;
        MaxTokens = maxTokens;
        Temperature = temperature;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}
