using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularyPronunciationEntity : Entity<Guid>
{
    public VocabularyPronunciationEntity() { Id = Guid.NewGuid(); }

    public Guid VocabularyId { get; set; }
    public string Accent { get; set; } = "US"; // UK | US
    public string Ipa { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string AudioSource { get; set; } = "BrowserTTS";
    public int? SyllableCount { get; set; }
    public string? StressPattern { get; set; }
}
