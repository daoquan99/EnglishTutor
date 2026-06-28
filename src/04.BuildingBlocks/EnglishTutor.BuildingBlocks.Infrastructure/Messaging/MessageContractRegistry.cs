namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

internal sealed class MessageContractRegistry : IMessageContractRegistry
{
    private readonly IReadOnlyDictionary<Type, MessageContractDescriptor> _byType;
    private readonly IReadOnlyDictionary<string, MessageContractDescriptor> _byName;

    public MessageContractRegistry(IEnumerable<MessageContractDescriptor> descriptors)
    {
        var materialized = descriptors.ToArray();
        _byType = materialized.ToDictionary(descriptor => descriptor.MessageType);
        _byName = materialized.ToDictionary(
            descriptor => descriptor.ContractName,
            StringComparer.Ordinal);
    }

    public MessageContractDescriptor Get(Type messageType) =>
        _byType.TryGetValue(messageType, out var descriptor)
            ? descriptor
            : throw new InvalidOperationException($"No messaging contract is registered for {messageType.FullName}.");

    public MessageContractDescriptor Get(string contractName) =>
        _byName.TryGetValue(contractName, out var descriptor)
            ? descriptor
            : throw new InvalidOperationException($"Unknown messaging contract '{contractName}'.");
}
