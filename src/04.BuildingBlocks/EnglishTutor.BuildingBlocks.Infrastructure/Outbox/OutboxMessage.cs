namespace EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Default implementation of <see cref="IOutboxMessage"/>.
/// EF Core maps this entity to the <c>messaging.outbox_messages</c> table.
/// </summary>
/// <remarks>
/// <para>All mutations must go through the intent-revealing methods
/// (<see cref="MarkProcessed"/>, <see cref="MarkFailed"/>) so the rich-domain
/// invariants of the outbox are preserved.</para>
/// <para>EF Core hydrates and persists the entity via the private backing
/// fields declared below — direct property setters are intentionally private
/// so callers cannot bypass the lifecycle methods.</para>
/// </remarks>
public sealed class OutboxMessage : IOutboxMessage
{
    private Guid _id;
    private System.DateTime _occurredAtUtc;
    private string _moduleName = string.Empty;
    private string _type = string.Empty;
    private string _payloadJson = string.Empty;
    private Guid? _correlationId;
    private Guid? _causationId;
    private System.DateTime? _processedAtUtc;
    private string? _error;
    private int _retryCount;

    public Guid Id
    {
        get => _id;
        private set => _id = value;
    }

    public System.DateTime OccurredAtUtc
    {
        get => _occurredAtUtc;
        private set => _occurredAtUtc = value;
    }

    public string ModuleName
    {
        get => _moduleName;
        private set => _moduleName = value;
    }

    public string Type
    {
        get => _type;
        private set => _type = value;
    }

    public string PayloadJson
    {
        get => _payloadJson;
        private set => _payloadJson = value;
    }

    public Guid? CorrelationId
    {
        get => _correlationId;
        private set => _correlationId = value;
    }

    public Guid? CausationId
    {
        get => _causationId;
        private set => _causationId = value;
    }

    public System.DateTime? ProcessedAtUtc
    {
        get => _processedAtUtc;
        private set => _processedAtUtc = value;
    }

    public string? Error
    {
        get => _error;
        private set => _error = value;
    }

    public int RetryCount
    {
        get => _retryCount;
        private set => _retryCount = value;
    }

    private OutboxMessage()
    {
    }

    public OutboxMessage(
        Guid id,
        System.DateTime occurredAtUtc,
        string moduleName,
        string type,
        string payloadJson,
        Guid? correlationId = null,
        Guid? causationId = null)
    {
        if (string.IsNullOrWhiteSpace(moduleName))
        {
            throw new ArgumentException("Module name is required.", nameof(moduleName));
        }
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentException("Event type is required.", nameof(type));
        }

        Id = id;
        OccurredAtUtc = occurredAtUtc;
        ModuleName = moduleName;
        Type = type;
        PayloadJson = payloadJson;
        CorrelationId = correlationId;
        CausationId = causationId;
        RetryCount = 0;
    }

    public void MarkProcessed(System.DateTime processedAtUtc)
    {
        ProcessedAtUtc = processedAtUtc;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
