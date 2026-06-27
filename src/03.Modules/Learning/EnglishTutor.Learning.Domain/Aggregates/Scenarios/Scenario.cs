using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Events;

namespace EnglishTutor.Learning.Domain.Aggregates.Scenarios;

/// <summary>
/// Aggregate root representing a practice scenario template (e.g. backend developer interview).
/// </summary>
public sealed class Scenario : AggregateRoot
{
    public Guid TopicId { get; private set; }
    public Guid ModeDefinitionId { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public string DifficultyLevel { get; private set; } = null!;
    public string PromptTemplate { get; private set; } = null!;
    public bool IsActive { get; private set; }

    private Scenario()
    {
    }

    public static Scenario Create(
        Guid topicId,
        Guid modeDefinitionId,
        string name,
        string? description,
        string difficultyLevel,
        string promptTemplate,
        Guid? createdByUserId = null)
    {
        if (topicId == Guid.Empty)
        {
            throw new ArgumentException("TopicId is required.", nameof(topicId));
        }
        if (modeDefinitionId == Guid.Empty)
        {
            throw new ArgumentException("ModeDefinitionId is required.", nameof(modeDefinitionId));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
        if (string.IsNullOrWhiteSpace(difficultyLevel))
        {
            throw new ArgumentException("Difficulty level is required.", nameof(difficultyLevel));
        }
        if (string.IsNullOrWhiteSpace(promptTemplate))
        {
            throw new ArgumentException("Prompt template is required.", nameof(promptTemplate));
        }

        var scenario = new Scenario
        {
            Id = Guid.NewGuid(),
            TopicId = topicId,
            ModeDefinitionId = modeDefinitionId,
            Name = name.Trim(),
            Description = description?.Trim(),
            DifficultyLevel = difficultyLevel.Trim(),
            PromptTemplate = promptTemplate.Trim(),
            IsActive = true
        };

        scenario.RaiseDomainEvent(new ScenarioCreatedDomainEvent(
            scenario.Id,
            scenario.TopicId,
            scenario.ModeDefinitionId,
            scenario.Name,
            scenario.DifficultyLevel,
            scenario.PromptTemplate,
            createdByUserId));

        return scenario;
    }

    public void UpdateDetails(
        string name,
        string? description,
        string difficultyLevel,
        string promptTemplate,
        Guid? updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required.", nameof(name));
        }
        if (string.IsNullOrWhiteSpace(difficultyLevel))
        {
            throw new ArgumentException("Difficulty level is required.", nameof(difficultyLevel));
        }
        if (string.IsNullOrWhiteSpace(promptTemplate))
        {
            throw new ArgumentException("Prompt template is required.", nameof(promptTemplate));
        }

        Name = name.Trim();
        Description = description?.Trim();
        DifficultyLevel = difficultyLevel.Trim();
        PromptTemplate = promptTemplate.Trim();

        RaiseDomainEvent(new ScenarioUpdatedDomainEvent(
            Id,
            TopicId,
            ModeDefinitionId,
            Name,
            DifficultyLevel,
            PromptTemplate,
            updatedByUserId));
    }

    public void Disable(Guid? disabledByUserId)
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;

        RaiseDomainEvent(new ScenarioDisabledDomainEvent(
            Id,
            disabledByUserId));
    }

    public void Enable()
    {
        IsActive = true;
    }
}
