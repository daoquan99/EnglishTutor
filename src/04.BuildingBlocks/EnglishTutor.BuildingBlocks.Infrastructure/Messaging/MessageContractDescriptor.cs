namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed record MessageContractDescriptor(
    Type MessageType,
    string ContractName,
    string ExchangeName,
    string RoutingKey);
