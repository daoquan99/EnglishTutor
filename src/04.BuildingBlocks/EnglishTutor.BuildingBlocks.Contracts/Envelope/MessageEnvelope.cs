namespace EnglishTutor.BuildingBlocks.Contracts.Envelope;

public sealed record MessageEnvelope<T> where T : class
{
    public T Message { get; init; } = default!;
    public Guid MessageId { get; init; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;
    public string MessageType { get; init; } = typeof(T).Name;
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
}
