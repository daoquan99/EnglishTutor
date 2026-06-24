using EnglishTutor.BuildingBlocks.Domain.ValueObjects;
using System.Text.RegularExpressions;

namespace EnglishTutor.Learning.Domain.Aggregates.Topics.ValueObjects;

/// <summary>
/// URL-friendly slug value object. Normalizes to lowercase and ensures only alphanumeric characters and hyphens.
/// </summary>
public sealed class Slug : ValueObject
{
    private static readonly Regex SlugRegex = new(@"^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Slug Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Slug cannot be empty.", nameof(raw));
        }

        var normalized = raw.Trim().ToLowerInvariant();

        if (!SlugRegex.IsMatch(normalized))
        {
            throw new ArgumentException($"'{raw}' is not a valid URL-friendly slug. Slugs must contain only lowercase alphanumeric characters and hyphens, and cannot start or end with a hyphen.", nameof(raw));
        }

        return new Slug(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
