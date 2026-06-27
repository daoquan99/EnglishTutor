using System;
using System.Security.Cryptography;
using System.Text;
using EnglishTutor.AiGateway.Application;
using EnglishTutor.AiGateway.Application.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace EnglishTutor.AiGateway.Infrastructure.Security;

/// <summary>
/// AES-256-GCM implementation of <see cref="IAiKeyProtector"/>. The 256-bit key
/// is derived from <c>AiGateway:EncryptionMasterKey</c> via SHA-256 so any
/// sufficiently strong configured secret works. Ciphertext layout is
/// <c>base64(nonce(12) || tag(16) || ciphertext)</c>. Secrets are never logged.
/// </summary>
internal sealed class AesGcmAiKeyProtector : IAiKeyProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;

    private readonly byte[] _key;

    public AesGcmAiKeyProtector(IOptions<AiGatewayOptions> options)
    {
        var master = options.Value.EncryptionMasterKey;
        if (string.IsNullOrWhiteSpace(master))
        {
            throw new InvalidOperationException(
                "AiGateway:EncryptionMasterKey must be configured to protect provider keys.");
        }

        _key = SHA256.HashData(Encoding.UTF8.GetBytes(master));
    }

    public string Encrypt(string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);

        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plainBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plainBytes, cipher, tag);

        var output = new byte[NonceSize + TagSize + cipher.Length];
        Buffer.BlockCopy(nonce, 0, output, 0, NonceSize);
        Buffer.BlockCopy(tag, 0, output, NonceSize, TagSize);
        Buffer.BlockCopy(cipher, 0, output, NonceSize + TagSize, cipher.Length);
        return Convert.ToBase64String(output);
    }

    public string Decrypt(string ciphertext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciphertext);

        var input = Convert.FromBase64String(ciphertext);
        if (input.Length < NonceSize + TagSize)
        {
            throw new CryptographicException("Ciphertext is malformed.");
        }

        var nonce = new byte[NonceSize];
        var tag = new byte[TagSize];
        var cipher = new byte[input.Length - NonceSize - TagSize];
        Buffer.BlockCopy(input, 0, nonce, 0, NonceSize);
        Buffer.BlockCopy(input, NonceSize, tag, 0, TagSize);
        Buffer.BlockCopy(input, NonceSize + TagSize, cipher, 0, cipher.Length);

        var plain = new byte[cipher.Length];
        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, cipher, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }

    public string Mask(string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);

        var last4 = plaintext.Length <= 4 ? plaintext : plaintext[^4..];
        return $"****{last4}";
    }
}
