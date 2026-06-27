namespace EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;

/// <summary>
/// Email value object. Validates format and normalizes to lowercase.
/// Equality is case-insensitive (always compares normalized value).
/// </summary>
public sealed class Email : EnglishTutor.BuildingBlocks.Domain.ValueObjects.ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new ArgumentException("Email cannot be empty.", nameof(raw));
        }

        var normalized = raw.Trim().ToLowerInvariant();

        if (!IsValidFormat(normalized))
        {
            throw new ArgumentException($"'{raw}' is not a valid email address.", nameof(raw));
        }

        return new Email(normalized);
    }

    private static bool IsValidFormat(string email)
    {
        // Pragmatic email check: local@domain.tld with at least one dot in domain.
        // Avoids heavy regex while catching obvious typos.
        var atIndex = email.IndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return false;
        }

        var domain = email[(atIndex + 1)..];
        return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
