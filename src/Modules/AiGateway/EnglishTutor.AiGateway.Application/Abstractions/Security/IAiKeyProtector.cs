namespace EnglishTutor.AiGateway.Application.Abstractions.Security;

/// <summary>
/// Protects provider API key secrets at rest. The plaintext secret is encrypted
/// before persistence and only decrypted at the last responsible moment inside
/// Infrastructure (never returned to callers, never logged).
/// </summary>
public interface IAiKeyProtector
{
    /// <summary>Encrypts a plaintext provider key secret for storage.</summary>
    string Encrypt(string plaintext);

    /// <summary>Decrypts a stored provider key secret. Infrastructure-only use.</summary>
    string Decrypt(string ciphertext);

    /// <summary>
    /// Produces a non-reversible display mask (e.g. <c>sk-…last4</c>) safe to
    /// return in API responses and logs.
    /// </summary>
    string Mask(string plaintext);
}
