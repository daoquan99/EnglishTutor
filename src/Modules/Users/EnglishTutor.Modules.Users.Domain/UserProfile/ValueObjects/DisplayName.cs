using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Users.Domain.ValueObjects;

public sealed class DisplayName : ValueObject
{
    public string Value { get; }

    private DisplayName(string value) => Value = value;

    public static DisplayName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Display name is required.");
        }

        var normalized = value.Trim();

        if (normalized.Length is < 2 or > 100)
        {
            throw new DomainException("Display name must be between 2 and 100 characters.");
        }

        return new DisplayName(normalized);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
