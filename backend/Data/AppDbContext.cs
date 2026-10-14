using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Existing Live Conversation Entities
    public DbSet<ConversationEntity> Conversations => Set<ConversationEntity>();
    public DbSet<ChatMessageEntity> Messages => Set<ChatMessageEntity>();
    public DbSet<LanguageCorrectionEntity> Corrections => Set<LanguageCorrectionEntity>();

    // New Vocabulary & Flashcard Domain Entities
    public DbSet<VocabularyEntity> Vocabularies => Set<VocabularyEntity>();
    public DbSet<VocabularySenseEntity> VocabularySenses => Set<VocabularySenseEntity>();
    public DbSet<VocabularyPronunciationEntity> VocabularyPronunciations => Set<VocabularyPronunciationEntity>();
    public DbSet<VocabularyRelationEntity> VocabularyRelations => Set<VocabularyRelationEntity>();
    public DbSet<VocabularyWordFamilyEntity> VocabularyWordFamilies => Set<VocabularyWordFamilyEntity>();
    public DbSet<VocabularyCollocationEntity> VocabularyCollocations => Set<VocabularyCollocationEntity>();
    public DbSet<VocabularyExampleEntity> VocabularyExamples => Set<VocabularyExampleEntity>();
    public DbSet<VocabularyGrammarPatternEntity> VocabularyGrammarPatterns => Set<VocabularyGrammarPatternEntity>();

    // AI Model Catalog & Generation Audit
    public DbSet<AiModelConfigEntity> AiModelCatalog => Set<AiModelConfigEntity>();
    public DbSet<VocabularyGenerationAuditEntity> VocabularyGenerationAudits => Set<VocabularyGenerationAuditEntity>();

    // Daily Learning & Spaced Repetition
    public DbSet<DailyVocabularyPlanEntity> DailyVocabularyPlans => Set<DailyVocabularyPlanEntity>();
    public DbSet<DailyVocabularyItemEntity> DailyVocabularyItems => Set<DailyVocabularyItemEntity>();
    public DbSet<UserVocabularyProgressEntity> UserVocabularyProgresses => Set<UserVocabularyProgressEntity>();
    public DbSet<VocabularyReviewAttemptEntity> VocabularyReviewAttempts => Set<VocabularyReviewAttemptEntity>();

    // Pronunciation Rubric & Assessments
    public DbSet<ScoringRubricEntity> ScoringRubrics => Set<ScoringRubricEntity>();
    public DbSet<ScoringRubricCriterionEntity> ScoringRubricCriteria => Set<ScoringRubricCriterionEntity>();
    public DbSet<AudioRecordingEntity> AudioRecordings => Set<AudioRecordingEntity>();
    public DbSet<PronunciationAssessmentEntity> PronunciationAssessments => Set<PronunciationAssessmentEntity>();
    public DbSet<PronunciationCriterionScoreEntity> PronunciationCriterionScores => Set<PronunciationCriterionScoreEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Live Conversations
        modelBuilder.Entity<ConversationEntity>(entity =>
        {
            entity.ToTable("Conversations");
            entity.HasKey(e => e.Id);
            entity.HasMany(e => e.Messages)
                  .WithOne()
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Corrections)
                  .WithOne()
                  .HasForeignKey(c => c.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessageEntity>(entity =>
        {
            entity.ToTable("Messages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ConversationId, e.TurnId, e.Speaker }).IsUnique();
            entity.HasIndex(e => new { e.ConversationId, e.SequenceOrder });
        });

        modelBuilder.Entity<LanguageCorrectionEntity>(entity =>
        {
            entity.ToTable("Corrections");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConversationId);
        });

        // 2. Vocabulary Aggregate
        modelBuilder.Entity<VocabularyEntity>(entity =>
        {
            entity.ToTable("Vocabularies");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.NormalizedWord, e.LanguageCode });
            entity.HasMany(e => e.Senses)
                  .WithOne()
                  .HasForeignKey(s => s.VocabularyId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Pronunciations)
                  .WithOne()
                  .HasForeignKey(p => p.VocabularyId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.WordFamilies)
                  .WithOne()
                  .HasForeignKey(w => w.VocabularyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<VocabularySenseEntity>(entity =>
        {
            entity.ToTable("VocabularySenses");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.VocabularyId, e.DisplayOrder });
            entity.HasMany(e => e.Relations)
                  .WithOne()
                  .HasForeignKey(r => r.SenseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Collocations)
                  .WithOne()
                  .HasForeignKey(c => c.SenseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Examples)
                  .WithOne()
                  .HasForeignKey(x => x.SenseId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.GrammarPatterns)
                  .WithOne()
                  .HasForeignKey(g => g.SenseId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // 3. AI Model Catalog
        modelBuilder.Entity<AiModelConfigEntity>(entity =>
        {
            entity.ToTable("AiModelCatalog");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ModelCode).IsUnique();
        });

        // 4. Daily Learning Plan & Progress
        modelBuilder.Entity<DailyVocabularyPlanEntity>(entity =>
        {
            entity.ToTable("DailyVocabularyPlans");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.LearningDate }).IsUnique();
            entity.HasMany(e => e.Items)
                  .WithOne()
                  .HasForeignKey(i => i.PlanId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DailyVocabularyItemEntity>(entity =>
        {
            entity.ToTable("DailyVocabularyItems");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PlanId, e.SenseId }).IsUnique();
        });

        modelBuilder.Entity<UserVocabularyProgressEntity>(entity =>
        {
            entity.ToTable("UserVocabularyProgresses");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.SenseId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.NextReviewAt });
        });

        // 5. Scoring Rubric & Pronunciation Assessments
        modelBuilder.Entity<ScoringRubricEntity>(entity =>
        {
            entity.ToTable("ScoringRubrics");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Code, e.Version }).IsUnique();
            entity.HasMany(e => e.Criteria)
                  .WithOne()
                  .HasForeignKey(c => c.RubricId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PronunciationAssessmentEntity>(entity =>
        {
            entity.ToTable("PronunciationAssessments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.VocabularyId, e.CreatedAt });
            entity.HasMany(e => e.CriterionScores)
                  .WithOne()
                  .HasForeignKey(c => c.AssessmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
