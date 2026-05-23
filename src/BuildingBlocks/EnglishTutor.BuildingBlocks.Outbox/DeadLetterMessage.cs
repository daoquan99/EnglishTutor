namespace EnglishTutor.BuildingBlocks.Outbox;

public enum DeadLetterStatus { Dead, Reprocessed }

public sealed class DeadLetterMessage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string SourceModule { get; set; } = string.Empty;
    public DateTime FailedAtUtc { get; set; }
    public int RetryCount { get; set; }
    public string LastError { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public DeadLetterStatus Status { get; set; } = DeadLetterStatus.Dead;
}
