namespace EnglishTutor.BuildingBlocks.Outbox;

public sealed class OutboxOptions
{
    public int ProcessingIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
    public int MaxRetryCount { get; set; } = 5;
    public int LockDurationSeconds { get; set; } = 30;
    public string WorkerInstanceId { get; set; } = Environment.MachineName;
}
