using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Pronunciation;

public sealed class AudioRecordingEntity : AggregateRoot<Guid>
{
    public AudioRecordingEntity() { Id = Guid.NewGuid(); }

    public string UserId { get; set; } = "default-learner";
    public string StorageKey { get; set; } = string.Empty;
    public string ContentType { get; set; } = "audio/webm";
    public long FileSizeBytes { get; set; }
    public int DurationMilliseconds { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }
    public string DeletionStatus { get; set; } = "Retained";
}
