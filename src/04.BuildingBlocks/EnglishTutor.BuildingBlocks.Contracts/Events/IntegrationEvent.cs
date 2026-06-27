namespace EnglishTutor.BuildingBlocks.Contracts.Events;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; init; } = DateTime.UtcNow;
    public string SchemaVersion { get; init; } = "1.0";
    public Guid? CorrelationId { get; init; }
    public Guid? CausationId { get; init; }
}
