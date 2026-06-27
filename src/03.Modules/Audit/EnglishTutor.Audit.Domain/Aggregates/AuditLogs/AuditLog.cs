using System;
using System.Text.Json;
using EnglishTutor.BuildingBlocks.Domain.Entities;

namespace EnglishTutor.Audit.Domain.Aggregates.AuditLogs;

public sealed class AuditLog : Entity
{
    private static readonly string[] SecretMarkers =
    [
        "password", "passphrase", "secret", "token", "refreshtoken",
        "accesstoken", "apikey", "privatekey", "authorization", "cookie", "set-cookie"
    ];

    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string DetailJson { get; private set; } = string.Empty;
    public string? IpAddressHash { get; private set; }
    public string? UserAgentHash { get; private set; }
    public Guid? CorrelationId { get; private set; }

    private AuditLog() { }

    public static AuditLog Create(
        Guid? userId,
        string action,
        string entityType,
        string entityId,
        string detailJson,
        string? ipAddressHash,
        string? userAgentHash,
        Guid? correlationId,
        DateTime createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action is required.", nameof(action));
        if (action.Length > 100)
            throw new ArgumentException("Action max length is 100.", nameof(action));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("EntityType is required.", nameof(entityType));
        if (entityType.Length > 200)
            throw new ArgumentException("EntityType max length is 200.", nameof(entityType));

        if (string.IsNullOrWhiteSpace(entityId))
            throw new ArgumentException("EntityId is required.", nameof(entityId));
        if (entityId.Length > 100)
            throw new ArgumentException("EntityId max length is 100.", nameof(entityId));

        if (string.IsNullOrWhiteSpace(detailJson))
            throw new ArgumentException("DetailJson is required.", nameof(detailJson));
        if (detailJson.Length > 50000)
            throw new ArgumentException("DetailJson max length is 50,000.", nameof(detailJson));

        if (createdAtUtc == default)
            throw new ArgumentException("CreatedAtUtc must be a valid UTC timestamp.", nameof(createdAtUtc));

        // Validate JSON format
        try
        {
            using var doc = JsonDocument.Parse(detailJson);
        }
        catch (JsonException ex)
        {
            throw new ArgumentException("DetailJson must be valid JSON.", nameof(detailJson), ex);
        }

        // Validate secret markers
        if (ContainsSecrets(detailJson))
        {
            throw new ArgumentException("DetailJson contains sensitive markers.", nameof(detailJson));
        }

        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            DetailJson = detailJson,
            IpAddressHash = ipAddressHash,
            UserAgentHash = userAgentHash,
            CorrelationId = correlationId
        };

        log.CreatedAtUtc = createdAtUtc;
        log.CreatedByUserId = userId;

        return log;
    }

    private static bool ContainsSecrets(string detailJson)
    {
        if (string.IsNullOrWhiteSpace(detailJson)) return false;
        var lower = detailJson.ToLowerInvariant();
        foreach (var marker in SecretMarkers)
        {
            if (lower.Contains(marker))
            {
                return true;
            }
        }
        return false;
    }
}
