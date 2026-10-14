using Microsoft.EntityFrameworkCore;

namespace EnglishLiveTutor.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ConversationEntity> Conversations => Set<ConversationEntity>();
    public DbSet<ChatMessageEntity> Messages => Set<ChatMessageEntity>();
    public DbSet<LanguageCorrectionEntity> Corrections => Set<LanguageCorrectionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ConversationEntity>(entity =>
        {
            entity.ToTable("Conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(64);
            entity.Property(e => e.Title).HasMaxLength(256).IsRequired();
            entity.Property(e => e.CefrLevel).HasMaxLength(16).IsRequired();
            entity.Property(e => e.VoiceName).HasMaxLength(64).IsRequired();

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
            entity.Property(e => e.ErrorCategory).HasMaxLength(64);
            entity.HasIndex(e => e.ConversationId);
        });
    }
}
