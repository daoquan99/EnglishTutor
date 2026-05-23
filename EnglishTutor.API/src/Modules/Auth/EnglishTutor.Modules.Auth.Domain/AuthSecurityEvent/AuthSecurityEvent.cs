using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent.Enums;

namespace EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;

public sealed class AuthSecurityEvent : AggregateRoot<Guid>
{
    public Guid AuthUserId { get; private set; }
    public Guid? SessionId { get; private set; }
    public string? DeviceId { get; private set; }
    public AuthSecurityEventType EventType { get; private set; }
    public AuthSecurityEventSeverity Severity { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? MetadataJson { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }
    public DateTime? ReviewedAtUtc { get; private set; }
    public Guid? ReviewedByAdminId { get; private set; }
    public string? ReviewNote { get; private set; }

    private AuthSecurityEvent() { }

    public static AuthSecurityEvent Create(
        Guid authUserId,
        Guid? sessionId,
        string? deviceId,
        AuthSecurityEventType eventType,
        AuthSecurityEventSeverity severity,
        string? ipAddress,
        string? userAgent,
        string? metadataJson,
        DateTime utcNow)
    {
        if (authUserId == Guid.Empty)
        {
            throw new DomainException("Auth user id is required.");
        }

        return new AuthSecurityEvent
        {
            Id = Guid.NewGuid(),
            AuthUserId = authUserId,
            SessionId = sessionId,
            DeviceId = NormalizeOptional(deviceId, 128),
            EventType = eventType,
            Severity = severity,
            IpAddress = NormalizeOptional(ipAddress, 64),
            UserAgent = NormalizeOptional(userAgent, 512),
            MetadataJson = NormalizeOptional(metadataJson, 4000),
            OccurredAtUtc = utcNow
        };
    }

    public void MarkReviewed(Guid adminUserId, string? note, DateTime utcNow)
    {
        if (adminUserId == Guid.Empty)
        {
            throw new DomainException("Admin user id is required.");
        }

        ReviewedAtUtc = utcNow;
        ReviewedByAdminId = adminUserId;
        ReviewNote = NormalizeOptional(note, 1000);
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
