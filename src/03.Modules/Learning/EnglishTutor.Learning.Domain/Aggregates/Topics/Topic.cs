using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Entities;
using EnglishTutor.Learning.Domain.Aggregates.Topics.ValueObjects;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Events;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics;

/// <summary>
/// Aggregate root representing a practice topic unit.
/// </summary>
public sealed class Topic : AggregateRoot
{
    private readonly List<TopicMode> _topicModes = [];

    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyCollection<TopicMode> TopicModes => _topicModes.AsReadOnly();

    private Topic()
    {
    }

    public static Topic Create(
        string name,
        string slug,
        string? description,
        Guid? createdByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        var topicSlug = Slug.Create(slug);

        var topic = new Topic
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = topicSlug,
            Description = description?.Trim(),
            IsActive = true
        };

        topic.RaiseDomainEvent(new TopicCreatedDomainEvent(
            topic.Id,
            topic.Name,
            topic.Slug.Value,
            createdByUserId));

        return topic;
    }

    public void UpdateDetails(
        string name,
        string slug,
        string? description,
        Guid? updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        var topicSlug = Slug.Create(slug);

        Name = name.Trim();
        Slug = topicSlug;
        Description = description?.Trim();

        RaiseDomainEvent(new TopicUpdatedDomainEvent(
            Id,
            Name,
            Slug.Value,
            Description ?? string.Empty,
            updatedByUserId));
    }

    public void Disable(Guid? disabledByUserId)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        RaiseDomainEvent(new TopicDisabledDomainEvent(
            Id,
            disabledByUserId));
    }

    public void Enable()
    {
        IsActive = true;
    }

    public TopicMode? EnableMode(
        Guid modeDefinitionId,
        string? configJson,
        Guid? userId)
    {
        if (modeDefinitionId == Guid.Empty)
        {
            throw new ArgumentException("ModeDefinitionId is required.", nameof(modeDefinitionId));
        }

        var existingMode = _topicModes.FirstOrDefault(tm => tm.ModeDefinitionId == modeDefinitionId);
        TopicMode? result = null;

        if (existingMode is not null)
        {
            existingMode.UpdateConfig(configJson);
        }
        else
        {
            var newMode = TopicMode.Create(Id, modeDefinitionId, configJson);
            _topicModes.Add(newMode);
            result = newMode;
        }

        RaiseDomainEvent(new TopicModeEnabledDomainEvent(
            Id,
            modeDefinitionId,
            configJson ?? string.Empty,
            userId));

        return result;
    }

    public void DisableMode(
        Guid modeDefinitionId,
        Guid? userId)
    {
        if (modeDefinitionId == Guid.Empty)
        {
            throw new ArgumentException("ModeDefinitionId is required.", nameof(modeDefinitionId));
        }

        var existingMode = _topicModes.FirstOrDefault(tm => tm.ModeDefinitionId == modeDefinitionId);

        if (existingMode is not null && existingMode.IsEnabled)
        {
            existingMode.Disable();
            RaiseDomainEvent(new TopicModeDisabledDomainEvent(
                Id,
                modeDefinitionId,
                userId));
        }
    }
}
