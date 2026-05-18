using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.AI.Domain.Enums;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class PromptTemplate : AggregateRoot<Guid>
{
    private readonly List<PromptVersion> _versions = [];

    public string Name { get; private set; } = string.Empty;
    public AiTaskType TaskType { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public IReadOnlyCollection<PromptVersion> Versions => _versions.AsReadOnly();

    private PromptTemplate() { }

    public static PromptTemplate Create(string name, AiTaskType taskType, string description)
    {
        return new PromptTemplate
        {
            Id = Guid.NewGuid(),
            Name = Normalize(name, 150, "Prompt template name"),
            TaskType = taskType,
            Description = Normalize(description, 500, "Prompt template description"),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public PromptVersion AddVersion(string systemPrompt, string userPromptTemplate, bool activate)
    {
        var nextVersion = _versions.Count == 0 ? 1 : _versions.Max(version => version.VersionNumber) + 1;

        if (activate)
        {
            foreach (var existingVersion in _versions)
            {
                existingVersion.Deactivate();
            }
        }

        var version = PromptVersion.Create(Id, nextVersion, systemPrompt, userPromptTemplate, activate);
        _versions.Add(version);
        return version;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string Normalize(string value, int maxLength, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainException($"{fieldName} must not exceed {maxLength} characters.");
        }

        return normalized;
    }
}
