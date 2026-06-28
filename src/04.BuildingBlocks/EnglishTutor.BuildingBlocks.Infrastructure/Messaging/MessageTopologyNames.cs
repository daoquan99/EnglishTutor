namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public static class MessageTopologyNames
{
    public const string IntegrationExchange = "english.integration";
    public const string CommandExchange = "english.commands";
    public const string RetryExchange = "english.retry";
    public const string DeadLetterExchange = "english.dlx";
    public const string UnroutableExchange = "english.unroutable";
    public const string UnroutableQueue = "english.unroutable";
}
