using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.AI.Domain.Entities;

public sealed class PromptVersion : Entity<Guid>
{
    public Guid PromptTemplateId { get; private set; }
    public int VersionNumber { get; private set; }
    public string SystemPrompt { get; private set; } = string.Empty;
    public string UserPromptTemplate { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    private PromptVersion() { }

    internal static PromptVersion Create(
        Guid promptTemplateId,
        int versionNumber,
        string systemPrompt,
        string userPromptTemplate,
        bool isActive,
        DateTime utcNow)
    {
        if (promptTemplateId == Guid.Empty)
        {
            throw new DomainException("Prompt template id is required.");
        }

        if (versionNumber <= 0)
        {
            throw new DomainException("Prompt version number must be positive.");
        }

        return new PromptVersion
        {
            Id = Guid.NewGuid(),
            PromptTemplateId = promptTemplateId,
            VersionNumber = versionNumber,
            SystemPrompt = Normalize(systemPrompt, "System prompt"),
            UserPromptTemplate = Normalize(userPromptTemplate, "User prompt template"),
            IsActive = isActive,
            CreatedAtUtc = utcNow
        };
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static string Normalize(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        return value.Trim();
    }
}
