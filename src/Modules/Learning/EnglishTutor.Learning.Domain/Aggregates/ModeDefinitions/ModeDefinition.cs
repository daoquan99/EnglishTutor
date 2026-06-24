using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.ValueObjects;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;

namespace EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;

/// <summary>
/// Aggregate root representing a system-wide practice mode (e.g. shadowing, role-play).
/// </summary>
public sealed class ModeDefinition : AggregateRoot
{
    public ModeCode Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }

    private ModeDefinition()
    {
    }

    public static ModeDefinition Create(
        string code,
        string name,
        string? description,
        Guid? createdByUserId)
    {
        var modeCode = ModeCode.Create(code);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        var mode = new ModeDefinition
        {
            Id = Guid.NewGuid(),
            Code = modeCode,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true
        };

        mode.RaiseDomainEvent(new ModeDefinitionCreatedDomainEvent(
            mode.Id,
            mode.Code.Value,
            mode.Name,
            createdByUserId));

        return mode;
    }

    public void UpdateDetails(
        string name,
        string? description,
        Guid? updatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        }

        Name = name.Trim();
        Description = description?.Trim();

        RaiseDomainEvent(new ModeDefinitionUpdatedDomainEvent(
            Id,
            Code.Value,
            Name,
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

        RaiseDomainEvent(new ModeDefinitionDisabledDomainEvent(
            Id,
            disabledByUserId));
    }

    public void Enable()
    {
        IsActive = true;
    }
}
