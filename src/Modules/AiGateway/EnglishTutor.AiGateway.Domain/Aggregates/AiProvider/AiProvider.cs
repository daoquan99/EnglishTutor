using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiProvider;

/// <summary>
/// Represents an AI Provider aggregate (e.g. OpenAI, Google).
/// </summary>
public class AiProvider : AggregateRoot
{
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public long Version { get; private set; }

    private AiProvider() { }

    public static AiProvider Create(Guid id, string name, string code, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Provider code cannot be empty.", nameof(code));

        return new AiProvider
        {
            Id = id,
            Name = name,
            Code = code.ToLowerInvariant().Trim(),
            IsActive = isActive,
            Version = 1
        };
    }

    public void Update(string name, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name cannot be empty.", nameof(name));

        Name = name;
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
