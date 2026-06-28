namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed record MessageConsumerDescriptor(
    string ConsumerName,
    string QueueName,
    Type MessageType,
    ushort PrefetchCount = 16,
    int Concurrency = 1,
    int MaxAttempts = 5,
    TimeSpan? RetryDelay = null)
{
    public TimeSpan EffectiveRetryDelay => RetryDelay ?? TimeSpan.FromSeconds(10);
}
