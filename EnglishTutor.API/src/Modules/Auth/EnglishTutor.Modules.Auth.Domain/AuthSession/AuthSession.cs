using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;

namespace EnglishTutor.Modules.Auth.Domain.AuthSession;

public sealed class AuthSession : AggregateRoot<Guid>
{
    public Guid AuthUserId { get; private set; }
    public string DeviceId { get; private set; } = string.Empty;
    public string? UserAgent { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime LastUsedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public DateTime? SuspiciousAtUtc { get; private set; }
    public string? SuspiciousReason { get; private set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;

    private AuthSession() { }

    public static AuthSession Create(Guid authUserId, string deviceId, string? userAgent, string? ipAddress, DateTime utcNow)
    {
        if (authUserId == Guid.Empty)
        {
            throw new DomainException("Auth user id is required.");
        }

        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new DomainException("Device id is required.");
        }

        return new AuthSession
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            DeviceId = NormalizeRequired(deviceId, 128, "Device id"),
            UserAgent = NormalizeOptional(userAgent, 512),
            IpAddress = NormalizeOptional(ipAddress, 64),
            LastUsedAtUtc = utcNow
        };
    }

    public void MarkUsed(DateTime utcNow)
    {
        if (IsRevoked)
        {
            throw new DomainException("Auth session is revoked.");
        }

        LastUsedAtUtc = utcNow;
    }

    public void MarkSuspicious(string reason, DateTime utcNow)
    {
        SuspiciousAtUtc = utcNow;
        SuspiciousReason = NormalizeRequired(reason, 300, "Suspicious reason");
    }

    public void Revoke(DateTime utcNow)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAtUtc = utcNow;
    }

    private static string NormalizeRequired(string value, int maxLength, string name)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
        {
            throw new DomainException($"{name} is required.");
        }

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }
}
