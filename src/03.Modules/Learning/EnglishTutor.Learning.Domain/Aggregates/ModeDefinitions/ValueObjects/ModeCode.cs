using EnglishTutor.BuildingBlocks.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.ValueObjects;

/// <summary>
/// Lookup code for a system-wide practice mode definition.
/// </summary>
public sealed class ModeCode : ValueObject
{
    private static readonly Regex CodeRegex = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; }

    private ModeCode(string value)
    {
        Value = value;
    }

    public static ModeCode Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Mode code cannot be empty.", nameof(raw));
        }

        var normalized = raw.Trim().ToLowerInvariant();

        if (!CodeRegex.IsMatch(normalized))
        {
            throw new ArgumentException($"'{raw}' is not a valid lookup code. Mode codes must contain only lowercase alphanumeric characters and hyphens.", nameof(raw));
        }

        return new ModeCode(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
