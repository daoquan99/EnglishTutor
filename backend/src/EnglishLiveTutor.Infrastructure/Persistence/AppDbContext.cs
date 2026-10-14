using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Domain.Common.Abstractions;
using EnglishLiveTutor.Domain.Conversations;
using EnglishLiveTutor.Domain.Identity;
using EnglishLiveTutor.Domain.Prompts;
using EnglishLiveTutor.Domain.TeacherVoices;
using EnglishLiveTutor.Domain.Learning;
using EnglishLiveTutor.Domain.Pronunciation;
using EnglishLiveTutor.Domain.Vocabulary;
using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Live Conversation Aggregate
    public DbSet<ConversationEntity> Conversations => Set<ConversationEntity>();
    public DbSet<ChatMessageEntity> Messages => Set<ChatMessageEntity>();
    public DbSet<LanguageCorrectionEntity> Corrections => Set<LanguageCorrectionEntity>();

    // Vocabulary Aggregate
    public DbSet<VocabularyEntity> Vocabularies => Set<VocabularyEntity>();
    public DbSet<VocabularySenseEntity> VocabularySenses => Set<VocabularySenseEntity>();
    public DbSet<VocabularyPronunciationEntity> VocabularyPronunciations => Set<VocabularyPronunciationEntity>();
    public DbSet<VocabularyRelationEntity> VocabularyRelations => Set<VocabularyRelationEntity>();
    public DbSet<VocabularyWordFamilyEntity> VocabularyWordFamilies => Set<VocabularyWordFamilyEntity>();
    public DbSet<VocabularyCollocationEntity> VocabularyCollocations => Set<VocabularyCollocationEntity>();
    public DbSet<VocabularyExampleEntity> VocabularyExamples => Set<VocabularyExampleEntity>();
    public DbSet<VocabularyGrammarPatternEntity> VocabularyGrammarPatterns => Set<VocabularyGrammarPatternEntity>();

    // AI Model Catalog & Audits
    public DbSet<AiModelConfigEntity> AiModelCatalog => Set<AiModelConfigEntity>();
    public DbSet<VocabularyGenerationAuditEntity> VocabularyGenerationAudits => Set<VocabularyGenerationAuditEntity>();

    // Daily Learning & Spaced Repetition Aggregate
    public DbSet<DailyVocabularyPlanEntity> DailyVocabularyPlans => Set<DailyVocabularyPlanEntity>();
    public DbSet<DailyVocabularyItemEntity> DailyVocabularyItems => Set<DailyVocabularyItemEntity>();
    public DbSet<UserVocabularyProgressEntity> UserVocabularyProgresses => Set<UserVocabularyProgressEntity>();
    public DbSet<VocabularyReviewAttemptEntity> VocabularyReviewAttempts => Set<VocabularyReviewAttemptEntity>();

    // Pronunciation Rubrics & Assessments
    public DbSet<ScoringRubricEntity> ScoringRubrics => Set<ScoringRubricEntity>();
    public DbSet<ScoringRubricCriterionEntity> ScoringRubricCriteria => Set<ScoringRubricCriterionEntity>();
    public DbSet<AudioRecordingEntity> AudioRecordings => Set<AudioRecordingEntity>();
    public DbSet<PronunciationAssessmentEntity> PronunciationAssessments => Set<PronunciationAssessmentEntity>();
    public DbSet<PronunciationCriterionScoreEntity> PronunciationCriterionScores => Set<PronunciationCriterionScoreEntity>();

    // Identity & RBAC
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<RoleEntity> Roles => Set<RoleEntity>();
    public DbSet<PermissionEntity> Permissions => Set<PermissionEntity>();
    public DbSet<UserRoleEntity> UserRoles => Set<UserRoleEntity>();
    public DbSet<RolePermissionEntity> RolePermissions => Set<RolePermissionEntity>();
    public DbSet<RefreshTokenEntity> RefreshTokens => Set<RefreshTokenEntity>();

    // Database-backed CEFR prompts and Google teacher voice catalog
    public DbSet<AiPromptTemplateEntity> AiPromptTemplates => Set<AiPromptTemplateEntity>();
    public DbSet<TeacherVoiceEntity> TeacherVoices => Set<TeacherVoiceEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Live Conversations
        modelBuilder.Entity<ConversationEntity>(entity =>
        {
            entity.ToTable("Conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.Title).HasMaxLength(256).IsRequired();
            entity.Property(e => e.CefrLevel).HasMaxLength(16).IsRequired();
            entity.Property(e => e.VoiceName).HasMaxLength(64).IsRequired();
            entity.Property(e => e.ConversationMode).HasMaxLength(64).IsRequired().HasDefaultValue("daily_conversation");
            entity.Property(e => e.Topic).HasMaxLength(512).IsRequired().HasDefaultValue(string.Empty);
            entity.Property(e => e.CustomSystemPrompt).HasColumnType("nvarchar(max)");

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
            entity.Property(e => e.Id).HasMaxLength(128);
            entity.Property(e => e.ConversationId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.TurnId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Speaker).HasMaxLength(16).IsRequired();
            entity.Property(e => e.TranscriptLine1).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.TranslationLine2).HasColumnType("nvarchar(max)").IsRequired();

            entity.HasIndex(e => new { e.ConversationId, e.TurnId, e.Speaker }).IsUnique();
            entity.HasIndex(e => new { e.ConversationId, e.SequenceOrder });
        });

        modelBuilder.Entity<LanguageCorrectionEntity>(entity =>
        {
            entity.ToTable("Corrections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.ConversationId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.TurnId).HasMaxLength(64);
            entity.Property(e => e.OriginalText).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.CorrectedText).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.ErrorCategory).HasMaxLength(64).IsRequired();
            entity.Property(e => e.ExplanationVi).HasColumnType("nvarchar(max)").IsRequired();
            entity.Property(e => e.CefrTip).HasColumnType("nvarchar(max)").IsRequired();

            entity.HasIndex(e => e.ConversationId);
        });

        // 2. Vocabulary Aggregate
        modelBuilder.Entity<VocabularyEntity>(entity =>
        {
            entity.ToTable("Vocabularies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Word).HasMaxLength(160).IsRequired();
            entity.Property(e => e.NormalizedWord).HasMaxLength(160).IsRequired();
            entity.Property(e => e.LanguageCode).HasMaxLength(16).HasDefaultValue("en").IsRequired();
            entity.Property(e => e.SourceType).HasMaxLength(32).HasDefaultValue("Seeded").IsRequired();
            entity.Property(e => e.SourceModelId).HasMaxLength(64);
            entity.Property(e => e.TopicCategory).HasMaxLength(128);

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
            entity.Property(e => e.PartOfSpeech).HasMaxLength(64).IsRequired();
            entity.Property(e => e.DefinitionEn).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.MeaningVi).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.DescriptionEn).HasMaxLength(1500);
            entity.Property(e => e.CefrLevel).HasMaxLength(8).HasDefaultValue("B1").IsRequired();

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

        modelBuilder.Entity<VocabularyPronunciationEntity>(entity =>
        {
            entity.ToTable("VocabularyPronunciations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Accent).HasMaxLength(16).HasDefaultValue("US").IsRequired();
            entity.Property(e => e.Ipa).HasMaxLength(160).IsRequired();
            entity.Property(e => e.AudioUrl).HasMaxLength(512);
            entity.Property(e => e.AudioSource).HasMaxLength(64).HasDefaultValue("BrowserTTS").IsRequired();
            entity.Property(e => e.StressPattern).HasMaxLength(64);
        });

        modelBuilder.Entity<VocabularyRelationEntity>(entity =>
        {
            entity.ToTable("VocabularyRelations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RelationType).HasMaxLength(32).IsRequired();
            entity.Property(e => e.RelatedWord).HasMaxLength(160).IsRequired();
        });

        modelBuilder.Entity<VocabularyWordFamilyEntity>(entity =>
        {
            entity.ToTable("VocabularyWordFamilies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RelatedWord).HasMaxLength(160).IsRequired();
            entity.Property(e => e.PartOfSpeech).HasMaxLength(64).IsRequired();
        });

        modelBuilder.Entity<VocabularyCollocationEntity>(entity =>
        {
            entity.ToTable("VocabularyCollocations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhraseEn).HasMaxLength(300).IsRequired();
            entity.Property(e => e.MeaningVi).HasMaxLength(300);
        });

        modelBuilder.Entity<VocabularyExampleEntity>(entity =>
        {
            entity.ToTable("VocabularyExamples");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SentenceEn).HasMaxLength(600).IsRequired();
            entity.Property(e => e.SentenceVi).HasMaxLength(600);
            entity.Property(e => e.AudioUrl).HasMaxLength(512);
            entity.Property(e => e.CefrLevel).HasMaxLength(8);
        });

        modelBuilder.Entity<VocabularyGrammarPatternEntity>(entity =>
        {
            entity.ToTable("VocabularyGrammarPatterns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Pattern).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Explanation).HasMaxLength(600);
            entity.Property(e => e.Example).HasMaxLength(600);
        });

        // 3. AI Model Catalog
        modelBuilder.Entity<AiModelConfigEntity>(entity =>
        {
            entity.ToTable("AiModelCatalog");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DisplayName).HasMaxLength(128).IsRequired();
            entity.Property(e => e.ModelCode).HasMaxLength(128).IsRequired();
            entity.Property(e => e.RoutedModelCode).HasMaxLength(128);
            entity.Property(e => e.CapabilityNote).HasMaxLength(500);

            entity.HasIndex(e => e.ModelCode).IsUnique();
        });

        modelBuilder.Entity<VocabularyGenerationAuditEntity>(entity =>
        {
            entity.ToTable("VocabularyGenerationAudits");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.ModelCode).HasMaxLength(128).IsRequired();
            entity.Property(e => e.PromptVersion).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();
        });

        // 4. Daily Learning Plan & Progress
        modelBuilder.Entity<DailyVocabularyPlanEntity>(entity =>
        {
            entity.ToTable("DailyVocabularyPlans");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.LearningDate).HasMaxLength(16).IsRequired();
            entity.Property(e => e.TimeZoneId).HasMaxLength(64).HasDefaultValue("Asia/Ho_Chi_Minh").IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).HasDefaultValue("Active").IsRequired();

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
            entity.Property(e => e.ItemType).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(32).IsRequired();

            entity.HasIndex(e => new { e.PlanId, e.SenseId }).IsUnique();
        });

        modelBuilder.Entity<UserVocabularyProgressEntity>(entity =>
        {
            entity.ToTable("UserVocabularyProgresses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.EaseFactor).HasColumnType("decimal(5,2)").HasDefaultValue(2.50m);
            entity.Property(e => e.LatestPronunciationScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.LearningStatus).HasMaxLength(32).HasDefaultValue("New").IsRequired();

            entity.HasIndex(e => new { e.UserId, e.SenseId }).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.NextReviewAt });
        });

        modelBuilder.Entity<VocabularyReviewAttemptEntity>(entity =>
        {
            entity.ToTable("VocabularyReviewAttempts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.ReviewType).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Rating).HasMaxLength(16).IsRequired();
        });

        // 5. Scoring Rubric & Pronunciation Assessments
        modelBuilder.Entity<ScoringRubricEntity>(entity =>
        {
            entity.ToTable("ScoringRubrics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(160).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.AssessmentType).HasMaxLength(32).HasDefaultValue("Pronunciation").IsRequired();

            entity.HasIndex(e => new { e.Code, e.Version }).IsUnique();

            entity.HasMany(e => e.Criteria)
                  .WithOne()
                  .HasForeignKey(c => c.RubricId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ScoringRubricCriterionEntity>(entity =>
        {
            entity.ToTable("ScoringRubricCriteria");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(64).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(128).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Weight).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(e => e.MinimumScore).HasColumnType("decimal(5,2)").HasDefaultValue(0m);
            entity.Property(e => e.MaximumScore).HasColumnType("decimal(5,2)").HasDefaultValue(100m);
        });

        modelBuilder.Entity<AudioRecordingEntity>(entity =>
        {
            entity.ToTable("AudioRecordings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.StorageKey).HasMaxLength(256).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(64).HasDefaultValue("audio/webm");
            entity.Property(e => e.DeletionStatus).HasMaxLength(32).HasDefaultValue("Retained");
        });

        modelBuilder.Entity<PronunciationAssessmentEntity>(entity =>
        {
            entity.ToTable("PronunciationAssessments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(64).IsRequired();
            entity.Property(e => e.TargetType).HasMaxLength(32).HasDefaultValue("Word").IsRequired();
            entity.Property(e => e.ReferenceText).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Accent).HasMaxLength(16).HasDefaultValue("US").IsRequired();
            entity.Property(e => e.ModelCode).HasMaxLength(128).IsRequired();
            entity.Property(e => e.RecognizedText).HasMaxLength(1000);
            entity.Property(e => e.OverallScore).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Confidence).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Status).HasMaxLength(32).HasDefaultValue("Completed").IsRequired();
            entity.Property(e => e.ErrorCode).HasMaxLength(64);

            entity.HasIndex(e => new { e.UserId, e.VocabularyId, e.CreatedAt });

            entity.HasMany(e => e.CriterionScores)
                  .WithOne()
                  .HasForeignKey(c => c.AssessmentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PronunciationCriterionScoreEntity>(entity =>
        {
            entity.ToTable("PronunciationCriterionScores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CriterionCode).HasMaxLength(64).IsRequired();
            entity.Property(e => e.CriterionName).HasMaxLength(128).IsRequired();
            entity.Property(e => e.CriterionDescriptionSnapshot).HasMaxLength(500);
            entity.Property(e => e.WeightSnapshot).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(e => e.RawScore).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(e => e.WeightedScore).HasColumnType("decimal(5,2)").IsRequired();
            entity.Property(e => e.Feedback).HasMaxLength(500);
        });

        // 6. Identity and refresh tokens
        modelBuilder.Entity<UserEntity>(e =>
        {
            e.ToTable("Users"); e.HasKey(x => x.Id);
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.NormalizedEmail).HasMaxLength(256).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(160).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
            e.HasIndex(x => x.NormalizedEmail).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });
        modelBuilder.Entity<RoleEntity>(e =>
        {
            e.ToTable("Roles"); e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(64).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });
        modelBuilder.Entity<PermissionEntity>(e =>
        {
            e.ToTable("Permissions"); e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(128).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });
        modelBuilder.Entity<UserRoleEntity>(e =>
        {
            e.ToTable("UserRoles"); e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();
            e.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<RoleEntity>().WithMany().HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<RolePermissionEntity>(e =>
        {
            e.ToTable("RolePermissions"); e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            e.HasOne<RoleEntity>().WithMany().HasForeignKey(x => x.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne<PermissionEntity>().WithMany().HasForeignKey(x => x.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<RefreshTokenEntity>(e =>
        {
            e.ToTable("RefreshTokens"); e.HasKey(x => x.Id);
            e.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => new { x.UserId, x.ExpiresAt });
            e.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // 7. Editable AI system instructions and Gemini teacher voices
        modelBuilder.Entity<AiPromptTemplateEntity>(e =>
        {
            e.ToTable("AiPromptTemplates"); e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(128).IsRequired();
            e.Property(x => x.Category).HasMaxLength(64).IsRequired();
            e.Property(x => x.Content).HasColumnType("nvarchar(max)").IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });
        modelBuilder.Entity<TeacherVoiceEntity>(e =>
        {
            e.ToTable("TeacherVoices"); e.HasKey(x => x.Id);
            e.Property(x => x.Code).HasMaxLength(64).IsRequired();
            e.Property(x => x.DisplayName).HasMaxLength(128).IsRequired();
            e.Property(x => x.Type).HasMaxLength(32).IsRequired();
            e.Property(x => x.Gender).HasMaxLength(16).IsRequired();
            e.Property(x => x.Persona).HasMaxLength(64).IsRequired();
            e.Property(x => x.Description).HasMaxLength(600).IsRequired();
            e.Property(x => x.LabelVi).HasMaxLength(128);
            e.Property(x => x.StyleVi).HasMaxLength(256);
            e.HasIndex(x => x.Code).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        // Logical deletion is a per-aggregate-root rule, not a blanket rule for child entities.
        modelBuilder.Entity<AiModelConfigEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<AudioRecordingEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ConversationEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<DailyVocabularyPlanEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<PronunciationAssessmentEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<ScoringRubricEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<UserVocabularyProgressEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VocabularyEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VocabularyGenerationAuditEntity>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<VocabularyReviewAttemptEntity>().HasQueryFilter(e => !e.IsDeleted);

        // Apply consistent metadata across all persisted domain entities.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(IAuditable).IsAssignableFrom(entityType.ClrType)) continue;
            var entity = modelBuilder.Entity(entityType.ClrType);
            entity.Property<string>(nameof(IAuditable.CreatedBy)).HasMaxLength(128);
            entity.Property<string>(nameof(IAuditable.UpdatedBy)).HasMaxLength(128);
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                entity.Property<string>(nameof(ISoftDeletable.DeletedBy)).HasMaxLength(128);
            }
        }
    }
}

public sealed class AppDbContextFactoryWrapper : IAppDbContextFactory
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public AppDbContextFactoryWrapper(IDbContextFactory<AppDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IAppDbContext> CreateDbContextAsync(CancellationToken ct = default)
    {
        return await _dbFactory.CreateDbContextAsync(ct);
    }
}
