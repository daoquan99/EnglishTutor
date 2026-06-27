using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule;

/// <summary>
/// Represents an AI routing rule mapping activity context to primary and fallback models.
/// </summary>
public class AiRoutingRule : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string ActivityType { get; private set; } = default!;
    public string TopicCode { get; private set; } = default!;
    public string ScenarioCode { get; private set; } = default!;
    public Guid PrimaryModelId { get; private set; }
    public Guid? FallbackModelId { get; private set; }
    public bool IsActive { get; private set; }
    public long Version { get; private set; }

    private AiRoutingRule() { }

    public static AiRoutingRule Create(
        Guid id,
        string name,
        string activityType,
        string topicCode,
        string scenarioCode,
        Guid primaryModelId,
        Guid? fallbackModelId,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(activityType))
            throw new ArgumentException("Activity type cannot be empty.", nameof(activityType));
        if (string.IsNullOrWhiteSpace(topicCode))
            throw new ArgumentException("Topic code cannot be empty.", nameof(topicCode));
        if (string.IsNullOrWhiteSpace(scenarioCode))
            throw new ArgumentException("Scenario code cannot be empty.", nameof(scenarioCode));
        if (primaryModelId == Guid.Empty)
            throw new ArgumentException("Primary model ID is required.", nameof(primaryModelId));

        return new AiRoutingRule
        {
            Id = id,
            Name = name,
            ActivityType = activityType.ToLowerInvariant().Trim(),
            TopicCode = topicCode.ToLowerInvariant().Trim(),
            ScenarioCode = scenarioCode.ToLowerInvariant().Trim(),
            PrimaryModelId = primaryModelId,
            FallbackModelId = fallbackModelId,
            IsActive = isActive,
            Version = 1
        };
    }

    public void Update(
        string name,
        string activityType,
        string topicCode,
        string scenarioCode,
        Guid primaryModelId,
        Guid? fallbackModelId,
        bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Rule name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(activityType))
            throw new ArgumentException("Activity type cannot be empty.", nameof(activityType));
        if (string.IsNullOrWhiteSpace(topicCode))
            throw new ArgumentException("Topic code cannot be empty.", nameof(topicCode));
        if (string.IsNullOrWhiteSpace(scenarioCode))
            throw new ArgumentException("Scenario code cannot be empty.", nameof(scenarioCode));
        if (primaryModelId == Guid.Empty)
            throw new ArgumentException("Primary model ID is required.", nameof(primaryModelId));

        Name = name;
        ActivityType = activityType.ToLowerInvariant().Trim();
        TopicCode = topicCode.ToLowerInvariant().Trim();
        ScenarioCode = scenarioCode.ToLowerInvariant().Trim();
        PrimaryModelId = primaryModelId;
        FallbackModelId = fallbackModelId;
        IsActive = isActive;
        Version++;
    }

    public void Activate()
    {
        IsActive = true;
        Version++;
    }

    public void Deactivate()
    {
        IsActive = false;
        Version++;
    }

    public void MarkDeleted(Guid? deletedByUserId = null)
    {
        base.MarkDeleted(deletedByUserId, DateTime.UtcNow);
        Version++;
    }
}
