namespace EnglishTutor.Identity.Application.Abstractions.Auth;

/// <summary>
/// Generates a new cryptographically-random opaque refresh-token value.
/// Returns the raw value (to be set on the client). The hash is computed
/// separately by <see cref="IRefreshTokenHasher"/> before persistence.
/// </summary>
public interface IRefreshTokenGenerator
{
    string Generate();
}
