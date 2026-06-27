namespace EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

/// <summary>
/// Contract for an outbox message stored in the database
/// until it is reliably published to the message broker.
/// </summary>
/// <remarks>
/// <para><b>ModuleName</b> identifies the owning module (e.g. "Practice", "Learning")
/// so the publisher can route the message to module-specific queues and so
/// operators can filter outbox rows by module.</para>
/// <para><b>CorrelationId</b> / <b>CausationId</b> are <see cref="Guid"/> to align
/// with <see cref="EnglishTutor.BuildingBlocks.Contracts.Events.IntegrationEvent"/>
/// and <see cref="EnglishTutor.BuildingBlocks.Contracts.Envelope.MessageEnvelope{T}"/>.
/// </para>
/// </remarks>
public interface IOutboxMessage
{
    Guid Id { get; }
    System.DateTime OccurredAtUtc { get; }
    /// <summary>Owning module code (e.g. "Practice", "Learning", "Identity").</summary>
    string ModuleName { get; }
    /// <summary>CLR type name of the event payload.</summary>
    string Type { get; }
    /// <summary>JSON-serialized event payload.</summary>
    string PayloadJson { get; }
    Guid? CorrelationId { get; }
    Guid? CausationId { get; }
    System.DateTime? ProcessedAtUtc { get; }
    string? Error { get; }
    int RetryCount { get; }
}
