using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.ValueObjects;

public sealed class HashedPassword : ValueObject
{
    public string Value { get; }

    private HashedPassword(string value) => Value = value;

    public static HashedPassword Create(string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new DomainException("Hashed password is required.");
        }

        return new HashedPassword(hashedPassword);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator string(HashedPassword hp) => hp.Value;
}
