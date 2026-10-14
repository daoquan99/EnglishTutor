namespace EnglishLiveTutor.Domain.Vocabulary;

public enum VocabularySourceType
{
    Seeded = 0,
    AI = 1,
    Imported = 2,
    Manual = 3
}

public sealed class VocabularyEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Word { get; set; } = string.Empty;
    public string NormalizedWord { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "en";
    public int? FrequencyRank { get; set; }
    public string SourceType { get; set; } = "Seeded"; // Seeded | AI | Imported | Manual
    public string? SourceModelId { get; set; }
    public string? TopicCategory { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
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

public sealed class VocabularySenseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VocabularyId { get; set; }
    public string PartOfSpeech { get; set; } = "noun";
    public string DefinitionEn { get; set; } = string.Empty;
    public string MeaningVi { get; set; } = string.Empty;
    public string? DescriptionEn { get; set; }
    public string CefrLevel { get; set; } = "B1"; // A1, A2, B1, B2, C1, C2
    public bool IsPrimary { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;

    public List<VocabularyRelationEntity> Relations { get; set; } = new();
    public List<VocabularyCollocationEntity> Collocations { get; set; } = new();
    public List<VocabularyExampleEntity> Examples { get; set; } = new();
    public List<VocabularyGrammarPatternEntity> GrammarPatterns { get; set; } = new();
}

public sealed class VocabularyPronunciationEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VocabularyId { get; set; }
    public string Accent { get; set; } = "US"; // UK | US
    public string Ipa { get; set; } = string.Empty;
    public string? AudioUrl { get; set; }
    public string AudioSource { get; set; } = "BrowserTTS";
    public int? SyllableCount { get; set; }
    public string? StressPattern { get; set; }
}

public sealed class VocabularyRelationEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenseId { get; set; }
    public string RelationType { get; set; } = "Synonym"; // Synonym | Antonym
    public string RelatedWord { get; set; } = string.Empty;
    public Guid? RelatedVocabularyId { get; set; }
    public Guid? RelatedSenseId { get; set; }
    public int DisplayOrder { get; set; } = 1;
}

public sealed class VocabularyWordFamilyEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid VocabularyId { get; set; }
    public string RelatedWord { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = "noun";
    public Guid? RelatedVocabularyId { get; set; }
}

public sealed class VocabularyCollocationEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenseId { get; set; }
    public string PhraseEn { get; set; } = string.Empty;
    public string? MeaningVi { get; set; }
    public int DisplayOrder { get; set; } = 1;
}

public sealed class VocabularyExampleEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenseId { get; set; }
    public string SentenceEn { get; set; } = string.Empty;
    public string? SentenceVi { get; set; }
    public string? AudioUrl { get; set; }
    public string? CefrLevel { get; set; }
    public int DisplayOrder { get; set; } = 1;
}

public sealed class VocabularyGrammarPatternEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SenseId { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string? Example { get; set; }
}

public sealed class AiModelConfigEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string DisplayName { get; set; } = string.Empty;
    public string ModelCode { get; set; } = string.Empty;
    public string? RoutedModelCode { get; set; }
    public bool SupportsTextGeneration { get; set; }
    public bool SupportsStructuredOutput { get; set; }
    public bool SupportsAudioInput { get; set; }
    public bool SupportsPronunciationAssessment { get; set; }
    public bool SupportsLiveAudio { get; set; }
    public bool SupportsTranscription { get; set; }
    public bool SupportsTranslation { get; set; }
    public bool SupportsTts { get; set; }
    public bool SupportsStreaming { get; set; }
    public bool IsAvailable { get; set; } = true;
    public bool IsDeprecated { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string? CapabilityNote { get; set; }
    public int DisplayOrder { get; set; }

    public string GetEffectiveModelCode()
    {
        return IsDeprecated && !string.IsNullOrWhiteSpace(RoutedModelCode)
            ? RoutedModelCode!
            : ModelCode;
    }
}

public sealed class VocabularyGenerationAuditEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = "default-learner";
    public string ModelCode { get; set; } = string.Empty;
    public string PromptVersion { get; set; } = "VOCAB_GEN_V1";
    public int RequestedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }
    public string Status { get; set; } = "Completed";
    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
