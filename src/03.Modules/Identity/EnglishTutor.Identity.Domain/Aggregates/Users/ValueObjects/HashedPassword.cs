namespace EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;

/// <summary>
/// Hash value object wrapping the result of password hashing.
/// The plaintext password is never stored — only the hash produced by
/// <c>Microsoft.AspNetCore.Identity.PasswordHasher</c>.
/// </summary>
/// <remarks>
/// Equality is based on the hash string. Two <c>HashedPassword</c> instances
/// are equal iff their hash strings are equal (which means the same password
/// was hashed with the same salt — salts are baked into the hash string).
/// </remarks>
public sealed class HashedPassword : EnglishTutor.BuildingBlocks.Domain.ValueObjects.ValueObject
{
    public string Hash { get; }

    private HashedPassword(string hash)
    {
        Hash = hash;
    }

    /// <summary>
    /// Wraps an already-computed hash string. Used by the password hasher
    /// implementation when rehydrating from persistence.
    /// </summary>
    public static HashedPassword FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
        {
            throw new ArgumentException("Hash cannot be empty.", nameof(hash));
        }
        return new HashedPassword(hash);
    }

    /// <summary>
    /// Convenience for unit tests / seed: wrap a hash that was just produced
    /// by <c>IPasswordHasher.HashPassword</c>.
    /// </summary>
    public static HashedPassword FromNewHash(string hash)
        => FromHash(hash);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Hash;
    }

    public override string ToString() => "***REDACTED***";
}
