using EnglishTutor.BuildingBlocks.Domain.ValueObjects;

namespace EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.ValueObjects;

public sealed class LanguageCode : ValueObject
{
    public string Value { get; }

    private LanguageCode(string value)
    {
        Value = value;
    }

    public static LanguageCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Language code is required.", nameof(value));
        }

        var parts = value.Trim().Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 ||
            parts[0].Length is < 2 or > 8 ||
            !parts[0].All(char.IsAsciiLetter) ||
            parts.Any(part => part.Length is < 1 or > 8 || !part.All(char.IsAsciiLetterOrDigit)))
        {
            throw new ArgumentException("Language code must be a valid bounded BCP 47 tag.", nameof(value));
        }

        var canonical = new string[parts.Length];
        canonical[0] = parts[0].ToLowerInvariant();

        for (var index = 1; index < parts.Length; index++)
        {
            var part = parts[index];
            canonical[index] = part.Length switch
            {
                2 when part.All(char.IsAsciiLetter) => part.ToUpperInvariant(),
                4 when part.All(char.IsAsciiLetter) =>
                    char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant(),
                _ => part.ToLowerInvariant()
            };
        }

        var result = string.Join('-', canonical);
        if (result.Length > 35)
        {
            throw new ArgumentException("Language code cannot exceed 35 characters.", nameof(value));
        }

        return new LanguageCode(result);
    }

    public static implicit operator string(LanguageCode code) => code.Value;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
