using EnglishLiveTutor.Domain.Conversations;
using EnglishLiveTutor.Domain.Learning;
using EnglishLiveTutor.Domain.Pronunciation;
using EnglishLiveTutor.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Application.Common.Interfaces;

public interface IAppDbContext : IAsyncDisposable, IDisposable
{
    DbSet<ConversationEntity> Conversations { get; }
    DbSet<ChatMessageEntity> Messages { get; }
    DbSet<LanguageCorrectionEntity> Corrections { get; }

    DbSet<VocabularyEntity> Vocabularies { get; }
    DbSet<VocabularySenseEntity> VocabularySenses { get; }
    DbSet<VocabularyPronunciationEntity> VocabularyPronunciations { get; }
    DbSet<VocabularyRelationEntity> VocabularyRelations { get; }
    DbSet<VocabularyWordFamilyEntity> VocabularyWordFamilies { get; }
    DbSet<VocabularyCollocationEntity> VocabularyCollocations { get; }
    DbSet<VocabularyExampleEntity> VocabularyExamples { get; }
    DbSet<VocabularyGrammarPatternEntity> VocabularyGrammarPatterns { get; }

    DbSet<AiModelConfigEntity> AiModelCatalog { get; }
    DbSet<VocabularyGenerationAuditEntity> VocabularyGenerationAudits { get; }

    DbSet<DailyVocabularyPlanEntity> DailyVocabularyPlans { get; }
    DbSet<DailyVocabularyItemEntity> DailyVocabularyItems { get; }
    DbSet<UserVocabularyProgressEntity> UserVocabularyProgresses { get; }
    DbSet<VocabularyReviewAttemptEntity> VocabularyReviewAttempts { get; }

    DbSet<ScoringRubricEntity> ScoringRubrics { get; }
    DbSet<ScoringRubricCriterionEntity> ScoringRubricCriteria { get; }
    DbSet<AudioRecordingEntity> AudioRecordings { get; }
    DbSet<PronunciationAssessmentEntity> PronunciationAssessments { get; }
    DbSet<PronunciationCriterionScoreEntity> PronunciationCriterionScores { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IAppDbContextFactory
{
    Task<IAppDbContext> CreateDbContextAsync(CancellationToken ct = default);
}
