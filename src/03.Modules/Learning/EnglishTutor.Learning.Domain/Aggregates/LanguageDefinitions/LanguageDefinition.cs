using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.ValueObjects;

namespace EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions;

public sealed class LanguageDefinition : AggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string EnglishName { get; private set; } = string.Empty;
    public string NativeName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public bool IsAvailableAsNative { get; private set; }
    public bool IsAvailableAsTarget { get; private set; }
    public int SortOrder { get; private set; }
    public long Version { get; private set; }

    private LanguageDefinition()
    {
    }

    public static LanguageDefinition Create(
        Guid id,
        string code,
        string englishName,
        string nativeName,
        bool isAvailableAsNative,
        bool isAvailableAsTarget,
        int sortOrder,
        bool isActive = true)
    {
        return new LanguageDefinition
        {
            Id = id,
            Code = LanguageCode.Create(code).Value,
            EnglishName = RequireName(englishName, nameof(englishName)),
            NativeName = RequireName(nativeName, nameof(nativeName)),
            IsAvailableAsNative = isAvailableAsNative,
            IsAvailableAsTarget = isAvailableAsTarget,
            SortOrder = sortOrder,
            IsActive = isActive,
            Version = 1
        };
    }

    public void Update(
        string englishName,
        string nativeName,
        bool isAvailableAsNative,
        bool isAvailableAsTarget,
        int sortOrder)
    {
        EnglishName = RequireName(englishName, nameof(englishName));
        NativeName = RequireName(nativeName, nameof(nativeName));
        IsAvailableAsNative = isAvailableAsNative;
        IsAvailableAsTarget = isAvailableAsTarget;
        SortOrder = sortOrder;
        Version++;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        Version++;
    }

    private static string RequireName(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Language name is required.", parameterName);
        }

        return value.Trim();
    }
}
