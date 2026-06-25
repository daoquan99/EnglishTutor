using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;

/// <summary>
/// Represents a route lease aggregate root.
/// </summary>
public class AiRouteLease : AggregateRoot
{
    public Guid UserId { get; private set; }
    public Guid RoutingRuleId { get; private set; }
    public Guid ModelId { get; private set; }
    public Guid ProviderKeyId { get; private set; }
    public AiRouteLeaseStatus Status { get; private set; }
    public DateTime ExpiryAtUtc { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public long Version { get; private set; }

    private AiRouteLease() { }

    public static AiRouteLease Create(
        Guid id,
        Guid userId,
        Guid routingRuleId,
        Guid modelId,
        Guid providerKeyId,
        DateTime expiryAtUtc,
        string idempotencyKey)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));
        if (routingRuleId == Guid.Empty)
            throw new ArgumentException("Routing rule ID is required.", nameof(routingRuleId));
        if (modelId == Guid.Empty)
            throw new ArgumentException("Model ID is required.", nameof(modelId));
        if (providerKeyId == Guid.Empty)
            throw new ArgumentException("Provider key ID is required.", nameof(providerKeyId));
        if (expiryAtUtc <= DateTime.UtcNow)
            throw new ArgumentException("Expiry time must be in the future.", nameof(expiryAtUtc));
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));

        return new AiRouteLease
        {
            Id = id,
            UserId = userId,
            RoutingRuleId = routingRuleId,
            ModelId = modelId,
            ProviderKeyId = providerKeyId,
            Status = AiRouteLeaseStatus.Reserved,
            ExpiryAtUtc = expiryAtUtc,
            IdempotencyKey = idempotencyKey,
            Version = 1
        };
    }

    public void Confirm()
    {
        if (Status != AiRouteLeaseStatus.Reserved)
            throw new InvalidOperationException($"Cannot confirm lease in {Status} state.");
        if (ExpiryAtUtc < DateTime.UtcNow)
            throw new InvalidOperationException("Cannot confirm an expired lease.");

        Status = AiRouteLeaseStatus.Confirmed;
        Version++;
    }

    public void Release()
    {
        if (Status != AiRouteLeaseStatus.Reserved)
            throw new InvalidOperationException($"Cannot release lease in {Status} state.");

        Status = AiRouteLeaseStatus.Released;
        Version++;
    }

    public void Expire()
    {
        if (Status != AiRouteLeaseStatus.Reserved)
            throw new InvalidOperationException($"Cannot expire lease in {Status} state.");

        Status = AiRouteLeaseStatus.Expired;
        Version++;
    }

    public void MarkDeleted(Guid? deletedByUserId = null)
    {
        base.MarkDeleted(deletedByUserId, DateTime.UtcNow);
        Version++;
    }
}
