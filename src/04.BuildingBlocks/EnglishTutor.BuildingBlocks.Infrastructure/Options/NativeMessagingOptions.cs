using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Options;

public sealed class NativeMessagingOptions
{
    public const string SectionName = "Messaging:NativeRabbitMq";

    [Range(1, 1000)]
    public int OutboxBatchSize { get; set; } = 200;

    [Range(10, 60000)]
    public int OutboxPollIntervalMilliseconds { get; set; } = 50;

    [Range(5, 600)]
    public int LeaseSeconds { get; set; } = 30;

    [Range(1, 100)]
    public int MaxPublishAttempts { get; set; } = 10;

    [Range(1, 300)]
    public int PublishRetrySeconds { get; set; } = 5;

    [Range(1, 300)]
    public int PublisherConfirmTimeoutSeconds { get; set; } = 10;

    public string QueueType { get; set; } = "classic";
}
