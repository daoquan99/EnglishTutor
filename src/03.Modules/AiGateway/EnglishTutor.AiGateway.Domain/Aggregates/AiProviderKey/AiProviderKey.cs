using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;

/// <summary>
/// Represents an encrypted AI Provider API key aggregate.
/// </summary>
public class AiProviderKey : AggregateRoot
{
    public Guid ProviderId { get; private set; }
    public string Name { get; private set; } = default!;
    public string EncryptedKey { get; private set; } = default!;
    public string KeyMask { get; private set; } = default!;
    public int Priority { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? CooldownUntilUtc { get; private set; }
    public long Version { get; private set; }

    private AiProviderKey() { }

    public static AiProviderKey Create(
        Guid id, 
        Guid providerId, 
        string name, 
        string encryptedKey, 
        string keyMask, 
        int priority, 
        bool isActive)
    {
        if (providerId == Guid.Empty)
            throw new ArgumentException("Provider ID is required.", nameof(providerId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Key name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(encryptedKey))
            throw new ArgumentException("Encrypted key cannot be empty.", nameof(encryptedKey));
        if (string.IsNullOrWhiteSpace(keyMask))
            throw new ArgumentException("Key mask cannot be empty.", nameof(keyMask));
        if (priority < 0)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be non-negative.");

        return new AiProviderKey
        {
            Id = id,
            ProviderId = providerId,
            Name = name,
            EncryptedKey = encryptedKey,
            KeyMask = keyMask,
            Priority = priority,
            IsActive = isActive,
            CooldownUntilUtc = null,
            Version = 1
        };
    }

    public void Update(string name, string encryptedKey, string keyMask, int priority, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Key name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(encryptedKey))
            throw new ArgumentException("Encrypted key cannot be empty.", nameof(encryptedKey));
        if (string.IsNullOrWhiteSpace(keyMask))
            throw new ArgumentException("Key mask cannot be empty.", nameof(keyMask));
        if (priority < 0)
            throw new ArgumentOutOfRangeException(nameof(priority), "Priority must be non-negative.");

        Name = name;
        EncryptedKey = encryptedKey;
        KeyMask = keyMask;
        Priority = priority;
        IsActive = isActive;
        Version++;
    }

    public void SetCooldown(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(duration), "Cooldown duration must be positive.");

        CooldownUntilUtc = DateTime.UtcNow.Add(duration);
        Version++;
    }

    public void ClearCooldown()
    {
        CooldownUntilUtc = null;
        Version++;
    }

    public void Deactivate()
    {
        IsActive = false;
        Version++;
    }

    public void Quarantine()
    {
        IsActive = false;
        CooldownUntilUtc = null; // Clear cooldown since it is quarantined
        Version++;
    }

    public bool IsOnCooldown(DateTime now)
    {
        return CooldownUntilUtc.HasValue && CooldownUntilUtc.Value > now;
    }

    public void MarkDeleted(Guid? deletedByUserId = null)
    {
        base.MarkDeleted(deletedByUserId, DateTime.UtcNow);
        Version++;
    }
}
