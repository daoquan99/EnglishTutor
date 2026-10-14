using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Vocabulary;

public sealed class VocabularyEntity : AggregateRoot<Guid>
{
    public VocabularyEntity() { Id = Guid.NewGuid(); }

    public string Word { get; set; } = string.Empty;
    public string NormalizedWord { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "en";
    public int? FrequencyRank { get; set; }
    public string SourceType { get; set; } = "Seeded"; // Seeded | AI | Imported | Manual
    public string? SourceModelId { get; set; }
    public string? TopicCategory { get; set; }

    public bool IsActive { get; set; } = true;

    public List<VocabularySenseEntity> Senses { get; set; } = new();
    public List<VocabularyPronunciationEntity> Pronunciations { get; set; } = new();
    public List<VocabularyWordFamilyEntity> WordFamilies { get; set; } = new();

    public static string NormalizeWord(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        return string.Join(' ', raw.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
