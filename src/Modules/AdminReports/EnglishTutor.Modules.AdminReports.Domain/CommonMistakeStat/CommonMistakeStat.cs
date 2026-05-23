using EnglishTutor.BuildingBlocks.Domain;

namespace EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;

public sealed class CommonMistakeStat : Entity<Guid>
{
    public string TargetLanguageCode { get; private set; } = string.Empty;
    public string MistakeType { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public int OccurrenceCount { get; private set; }
    public int AffectedUsers { get; private set; }
    public string ExampleOriginal { get; private set; } = string.Empty;
    public string ExampleCorrected { get; private set; } = string.Empty;
    public DateTime LastUpdatedAtUtc { get; private set; }
}
