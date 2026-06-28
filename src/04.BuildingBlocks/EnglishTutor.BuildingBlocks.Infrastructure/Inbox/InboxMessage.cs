namespace EnglishTutor.BuildingBlocks.Infrastructure.Inbox;

public sealed class InboxMessage
{
    private InboxMessage() { }

    public InboxMessage(
        Guid messageId,
        string consumerName,
        string contractName,
        string schemaVersion,
        DateTimeOffset receivedAtUtc)
    {
        MessageId = messageId;
        ConsumerName = consumerName;
        ContractName = contractName;
        SchemaVersion = schemaVersion;
        ReceivedAtUtc = receivedAtUtc;
        CompletedAtUtc = receivedAtUtc;
    }

    public Guid MessageId { get; private set; }
    public string ConsumerName { get; private set; } = string.Empty;
    public string ContractName { get; private set; } = string.Empty;
    public string SchemaVersion { get; private set; } = string.Empty;
    public DateTimeOffset ReceivedAtUtc { get; private set; }
    public DateTimeOffset CompletedAtUtc { get; private set; }
}
