using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLiveTutor.Api.Data;

public enum VocabularySourceType
{
    Seeded = 0,
    AI = 1,
    Imported = 2,
    Manual = 3
}

[Table("Vocabularies")]
public sealed class VocabularyEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(160)]
    public string Word { get; set; } = string.Empty;

    [Required]
    [MaxLength(160)]
    public string NormalizedWord { get; set; } = string.Empty;

    [MaxLength(16)]
    public string LanguageCode { get; set; } = "en";

    public int? FrequencyRank { get; set; }

    [MaxLength(32)]
    public string SourceType { get; set; } = "Seeded"; // Seeded | AI | Imported | Manual

    [MaxLength(64)]
    public string? SourceModelId { get; set; }

    [MaxLength(128)]
    public string? TopicCategory { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public List<VocabularySenseEntity> Senses { get; set; } = new();
    public List<VocabularyPronunciationEntity> Pronunciations { get; set; } = new();
    public List<VocabularyWordFamilyEntity> WordFamilies { get; set; } = new();
}

[Table("VocabularySenses")]
public sealed class VocabularySenseEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VocabularyId { get; set; }

    [Required]
    [MaxLength(64)]
    public string PartOfSpeech { get; set; } = "noun";

    [Required]
    [MaxLength(1000)]
    public string DefinitionEn { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string MeaningVi { get; set; } = string.Empty;

    [MaxLength(1500)]
    public string? DescriptionEn { get; set; }

    [MaxLength(8)]
    public string CefrLevel { get; set; } = "B1"; // A1, A2, B1, B2, C1, C2

    public bool IsPrimary { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;

    public List<VocabularyRelationEntity> Relations { get; set; } = new();
    public List<VocabularyCollocationEntity> Collocations { get; set; } = new();
    public List<VocabularyExampleEntity> Examples { get; set; } = new();
    public List<VocabularyGrammarPatternEntity> GrammarPatterns { get; set; } = new();
}

[Table("VocabularyPronunciations")]
public sealed class VocabularyPronunciationEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VocabularyId { get; set; }

    [MaxLength(16)]
    public string Accent { get; set; } = "US"; // UK | US

    [MaxLength(160)]
    public string Ipa { get; set; } = string.Empty;

    [MaxLength(512)]
    public string? AudioUrl { get; set; }

    [MaxLength(64)]
    public string AudioSource { get; set; } = "BrowserTTS";

    public int? SyllableCount { get; set; }

    [MaxLength(64)]
    public string? StressPattern { get; set; }
}

[Table("VocabularyRelations")]
public sealed class VocabularyRelationEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SenseId { get; set; }

    [MaxLength(32)]
    public string RelationType { get; set; } = "Synonym"; // Synonym | Antonym

    [Required]
    [MaxLength(160)]
    public string RelatedWord { get; set; } = string.Empty;

    public Guid? RelatedVocabularyId { get; set; }
    public Guid? RelatedSenseId { get; set; }
    public int DisplayOrder { get; set; } = 1;
}

[Table("VocabularyWordFamilies")]
public sealed class VocabularyWordFamilyEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid VocabularyId { get; set; }

    [Required]
    [MaxLength(160)]
    public string RelatedWord { get; set; } = string.Empty;

    [MaxLength(64)]
    public string PartOfSpeech { get; set; } = string.Empty;

    public Guid? RelatedVocabularyId { get; set; }
}

[Table("VocabularyCollocations")]
public sealed class VocabularyCollocationEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SenseId { get; set; }

    [Required]
    [MaxLength(300)]
    public string PhraseEn { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? MeaningVi { get; set; }

    public int DisplayOrder { get; set; } = 1;
}

[Table("VocabularyExamples")]
public sealed class VocabularyExampleEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SenseId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string SentenceEn { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? SentenceVi { get; set; }

    [MaxLength(512)]
    public string? AudioUrl { get; set; }

    [MaxLength(8)]
    public string? CefrLevel { get; set; }

    public int DisplayOrder { get; set; } = 1;
}

[Table("VocabularyGrammarPatterns")]
public sealed class VocabularyGrammarPatternEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SenseId { get; set; }

    [Required]
    [MaxLength(300)]
    public string Pattern { get; set; } = string.Empty;

    [MaxLength(600)]
    public string? Explanation { get; set; }

    [MaxLength(600)]
    public string? Example { get; set; }
}

// ==============================================================================
// AI MODEL CATALOG & GENERATION TRACEABILITY (Sections 5 & 6.4)
// ==============================================================================
[Table("AiModelCatalog")]
public sealed class AiModelConfigEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(128)]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string ModelCode { get; set; } = string.Empty;

    [MaxLength(128)]
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

    [MaxLength(500)]
    public string? CapabilityNote { get; set; }

    public int DisplayOrder { get; set; }
}

[Table("VocabularyGenerationAudits")]
public sealed class VocabularyGenerationAuditEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(64)]
    public string UserId { get; set; } = "default-learner";

    [MaxLength(128)]
    public string ModelCode { get; set; } = string.Empty;

    [MaxLength(64)]
    public string PromptVersion { get; set; } = "VOCAB_GEN_V1";

    public int RequestedCount { get; set; }
    public int AcceptedCount { get; set; }
    public int RejectedCount { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "Completed";

    public string? ErrorMessage { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

// ==============================================================================
// DAILY VOCABULARY LEARNING & SPACED REPETITION (Section 7)
// ==============================================================================
[Table("DailyVocabularyPlans")]
public sealed class DailyVocabularyPlanEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string UserId { get; set; } = "default-learner";

    [Required]
    [MaxLength(16)]
    public string LearningDate { get; set; } = string.Empty; // yyyy-MM-dd in User TimeZone

    [MaxLength(64)]
    public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";

    public int NewVocabularyTarget { get; set; } = 20;

    [MaxLength(32)]
    public string Status { get; set; } = "Active"; // Active | Completed

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    public List<DailyVocabularyItemEntity> Items { get; set; } = new();
}

[Table("DailyVocabularyItems")]
public sealed class DailyVocabularyItemEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PlanId { get; set; }
    public Guid VocabularyId { get; set; }
    public Guid SenseId { get; set; }

    [MaxLength(32)]
    public string ItemType { get; set; } = "New"; // New | Review | CarryOver

    [MaxLength(32)]
    public string Status { get; set; } = "Pending"; // Pending | InProgress | Completed

    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}

[Table("UserVocabularyProgresses")]
public sealed class UserVocabularyProgressEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string UserId { get; set; } = "default-learner";

    public Guid VocabularyId { get; set; }
    public Guid SenseId { get; set; }

    public int MasteryLevel { get; set; } // 0..5
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int ReviewCount { get; set; }
    public DateTimeOffset? LastReviewedAt { get; set; }
    public DateTimeOffset? NextReviewAt { get; set; }
    public int IntervalDays { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal EaseFactor { get; set; } = 2.50m;

    [MaxLength(32)]
    public string LearningStatus { get; set; } = "New"; // New | Learning | Review | Mastered

    [Column(TypeName = "decimal(5,2)")]
    public decimal? LatestPronunciationScore { get; set; }
}

[Table("VocabularyReviewAttempts")]
public sealed class VocabularyReviewAttemptEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string UserId { get; set; } = "default-learner";

    public Guid VocabularyId { get; set; }
    public Guid SenseId { get; set; }

    [MaxLength(32)]
    public string ReviewType { get; set; } = "FlashcardRecall";

    [MaxLength(16)]
    public string Rating { get; set; } = "Good"; // Again | Hard | Good | Easy

    public DateTimeOffset ReviewedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? NextReviewAtSnapshot { get; set; }
}

// ==============================================================================
// PRONUNCIATION SCORING RUBRIC & ASSESSMENT (Sections 9, 10, 11)
// ==============================================================================
[Table("ScoringRubrics")]
public sealed class ScoringRubricEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = "ENGLISH_PRONUNCIATION_V1";

    [Required]
    [MaxLength(160)]
    public string Name { get; set; } = "English Pronunciation Rubric v1";

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    public int Version { get; set; } = 1;

    [MaxLength(32)]
    public string AssessmentType { get; set; } = "Pronunciation";

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public List<ScoringRubricCriterionEntity> Criteria { get; set; } = new();
}

[Table("ScoringRubricCriteria")]
public sealed class ScoringRubricCriterionEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RubricId { get; set; }

    [Required]
    [MaxLength(64)]
    public string Code { get; set; } = string.Empty; // Accuracy | Clarity | Fluency

    [Required]
    [MaxLength(128)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal Weight { get; set; } // 40.00 | 30.00 | 30.00

    [Column(TypeName = "decimal(5,2)")]
    public decimal MinimumScore { get; set; } = 0m;

    [Column(TypeName = "decimal(5,2)")]
    public decimal MaximumScore { get; set; } = 100m;

    public int DisplayOrder { get; set; }
    public bool IsEnabled { get; set; } = true;
}

[Table("AudioRecordings")]
public sealed class AudioRecordingEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string UserId { get; set; } = "default-learner";

    [Required]
    [MaxLength(256)]
    public string StorageKey { get; set; } = string.Empty;

    [MaxLength(64)]
    public string ContentType { get; set; } = "audio/webm";

    public long FileSizeBytes { get; set; }
    public int DurationMilliseconds { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ExpiresAt { get; set; }

    [MaxLength(32)]
    public string DeletionStatus { get; set; } = "Retained";
}

[Table("PronunciationAssessments")]
public sealed class PronunciationAssessmentEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(64)]
    public string UserId { get; set; } = "default-learner";

    public Guid VocabularyId { get; set; }
    public Guid SenseId { get; set; }

    [MaxLength(32)]
    public string TargetType { get; set; } = "Word"; // Word | Sentence

    public Guid? TargetId { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ReferenceText { get; set; } = string.Empty;

    [MaxLength(16)]
    public string Accent { get; set; } = "US";

    [MaxLength(128)]
    public string ModelCode { get; set; } = "gemini-3.8-flash";

    public Guid RubricId { get; set; }
    public int RubricVersion { get; set; } = 1;
    public Guid AudioRecordingId { get; set; }

    [MaxLength(1000)]
    public string? RecognizedText { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? OverallScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? Confidence { get; set; }

    [MaxLength(32)]
    public string Status { get; set; } = "Completed"; // Pending | Processing | Completed | NotAssessable | Failed

    public string? Feedback { get; set; }
    public string? StrengthsJson { get; set; }
    public string? ImprovementsJson { get; set; }
    public string? SuggestedPracticeJson { get; set; }

    [MaxLength(64)]
    public string? ErrorCode { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }

    public List<PronunciationCriterionScoreEntity> CriterionScores { get; set; } = new();
}

[Table("PronunciationCriterionScores")]
public sealed class PronunciationCriterionScoreEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssessmentId { get; set; }

    [Required]
    [MaxLength(64)]
    public string CriterionCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string CriterionName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string CriterionDescriptionSnapshot { get; set; } = string.Empty;

    [Column(TypeName = "decimal(5,2)")]
    public decimal WeightSnapshot { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal RawScore { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal WeightedScore { get; set; }

    [MaxLength(500)]
    public string Feedback { get; set; } = string.Empty;
}
