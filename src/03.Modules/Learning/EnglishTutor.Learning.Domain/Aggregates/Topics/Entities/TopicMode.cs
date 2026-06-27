using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.Entities;

/// <summary>
/// Child entity representing the configuration and enabled state of a practice mode within a specific topic.
/// </summary>
public sealed class TopicMode : Entity
{
    public Guid TopicId { get; private set; }
    public Guid ModeDefinitionId { get; private set; }
    public bool IsEnabled { get; private set; }
    public string? ConfigJson { get; private set; }

    private TopicMode()
    {
    }

    internal static TopicMode Create(
        Guid topicId,
        Guid modeDefinitionId,
        string? configJson)
    {
        if (topicId == Guid.Empty)
        {
            throw new ArgumentException("TopicId is required.", nameof(topicId));
        }
        if (modeDefinitionId == Guid.Empty)
        {
            throw new ArgumentException("ModeDefinitionId is required.", nameof(modeDefinitionId));
        }

        return new TopicMode
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            ModeDefinitionId = modeDefinitionId,
            IsEnabled = true,
            ConfigJson = configJson?.Trim()
        };
    }

    internal void UpdateConfig(string? configJson)
    {
        ConfigJson = configJson?.Trim();
        IsEnabled = true;
    }

    internal void Disable()
    {
        IsEnabled = false;
    }

    internal void Enable()
    {
        IsEnabled = true;
    }
}
