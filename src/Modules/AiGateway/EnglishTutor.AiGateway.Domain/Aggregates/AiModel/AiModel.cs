using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiModel;

/// <summary>
/// Represents an AI Model aggregate (e.g. gpt-4o, gemini-1.5-pro).
/// </summary>
public class AiModel : AggregateRoot
{
    public Guid ProviderId { get; private set; }
    public string Name { get; private set; } = default!;
    public string Code { get; private set; } = default!;
    public string[] Capabilities { get; private set; } = [];
    public bool IsActive { get; private set; }
    public long Version { get; private set; }

    private AiModel() { }

    public static AiModel Create(Guid id, Guid providerId, string name, string code, string[] capabilities, bool isActive)
    {
        if (providerId == Guid.Empty)
            throw new ArgumentException("Provider ID is required.", nameof(providerId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Model name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Model code cannot be empty.", nameof(code));

        return new AiModel
        {
            Id = id,
            ProviderId = providerId,
            Name = name,
            Code = code.ToLowerInvariant().Trim(),
            Capabilities = capabilities ?? [],
            IsActive = isActive,
            Version = 1
        };
    }

    public void Update(string name, string[] capabilities, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Model name cannot be empty.", nameof(name));

        Name = name;
        Capabilities = capabilities ?? [];
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
