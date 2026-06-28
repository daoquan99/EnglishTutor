using EnglishTutor.BuildingBlocks.Contracts.Events;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage() { }

    private OutboxMessage(
        IntegrationEvent integrationEvent,
        MessageContractDescriptor descriptor,
        string payloadJson,
        DateTimeOffset createdAtUtc)
    {
        Id = integrationEvent.EventId;
        ContractName = descriptor.ContractName;
        SchemaVersion = integrationEvent.SchemaVersion;
        ExchangeName = descriptor.ExchangeName;
        RoutingKey = descriptor.RoutingKey;
        OccurredAtUtc = new DateTimeOffset(integrationEvent.OccurredAtUtc, TimeSpan.Zero);
        PayloadJson = payloadJson;
        CorrelationId = integrationEvent.CorrelationId;
        CausationId = integrationEvent.CausationId;
        Status = OutboxMessageStatus.Pending;
        NextAttemptAtUtc = createdAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }
    public string ContractName { get; private set; } = string.Empty;
    public string SchemaVersion { get; private set; } = string.Empty;
    public string ExchangeName { get; private set; } = string.Empty;
    public string RoutingKey { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public string PayloadJson { get; private set; } = string.Empty;
    public Guid? CorrelationId { get; private set; }
    public Guid? CausationId { get; private set; }
    public OutboxMessageStatus Status { get; private set; }
    public int AttemptCount { get; private set; }
    public DateTimeOffset NextAttemptAtUtc { get; private set; }
    public Guid? LockId { get; private set; }
    public DateTimeOffset? LockedUntilUtc { get; private set; }
    public DateTimeOffset? PublishedAtUtc { get; private set; }
    public string? LastErrorCode { get; private set; }
    public string? LastErrorMessage { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static OutboxMessage Create(
        IntegrationEvent integrationEvent,
        MessageContractDescriptor descriptor,
        string payloadJson,
        DateTimeOffset createdAtUtc) =>
        new(integrationEvent, descriptor, payloadJson, createdAtUtc);

    public void MarkPublished(Guid lockId, DateTimeOffset publishedAtUtc)
    {
        EnsureLock(lockId);
        Status = OutboxMessageStatus.Published;
        PublishedAtUtc = publishedAtUtc;
        LockId = null;
        LockedUntilUtc = null;
        LastErrorCode = null;
        LastErrorMessage = null;
    }

    public void MarkFailed(
        Guid lockId,
        string errorCode,
        string errorMessage,
        DateTimeOffset nextAttemptAtUtc,
        int maxAttempts)
    {
        EnsureLock(lockId);
        AttemptCount++;
        Status = AttemptCount >= maxAttempts
            ? OutboxMessageStatus.Dead
            : OutboxMessageStatus.Failed;
        NextAttemptAtUtc = nextAttemptAtUtc;
        LastErrorCode = errorCode;
        LastErrorMessage = errorMessage.Length <= 1000
            ? errorMessage
            : errorMessage[..1000];
        LockId = null;
        LockedUntilUtc = null;
    }

    private void EnsureLock(Guid lockId)
    {
        if (LockId != lockId || Status != OutboxMessageStatus.Publishing)
        {
            throw new InvalidOperationException("The outbox message is not owned by the current dispatcher lease.");
        }
    }
}
