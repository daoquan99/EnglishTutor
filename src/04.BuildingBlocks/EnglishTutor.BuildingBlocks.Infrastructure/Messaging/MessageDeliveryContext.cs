namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed record MessageDeliveryContext(
    Guid MessageId,
    string ContractName,
    string SchemaVersion,
    Guid? CorrelationId,
    Guid? CausationId,
    int Attempt,
    DateTimeOffset ReceivedAtUtc);
