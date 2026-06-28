namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public interface IMessageContractRegistry
{
    MessageContractDescriptor Get(Type messageType);
    MessageContractDescriptor Get(string contractName);
}
