using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Configurations;

internal sealed class LearnerLanguagePortfolioConfiguration
    : IEntityTypeConfiguration<LearnerLanguagePortfolio>
{
    public void Configure(EntityTypeBuilder<LearnerLanguagePortfolio> builder)
    {
        builder.ToTable("learner_language_portfolios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(x => x.ActiveLanguagePairId).HasColumnName("active_language_pair_id");
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(x => x.UserId).IsUnique();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");
        builder.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");
        builder.Ignore(x => x.DomainEvents);

        builder.OwnsMany(x => x.LanguagePairs, pair =>
        {
            pair.ToTable("learner_language_pairs");
            pair.WithOwner().HasForeignKey(x => x.PortfolioId);
            pair.HasKey(x => x.Id);
            pair.Property(x => x.Id).HasColumnName("id");
            pair.Property(x => x.PortfolioId).HasColumnName("portfolio_id");
            pair.Property(x => x.NativeLanguageCode).HasColumnName("native_language_code").HasMaxLength(35).IsRequired();
            pair.Property(x => x.TargetLanguageCode).HasColumnName("target_language_code").HasMaxLength(35).IsRequired();
            pair.Property(x => x.ExplanationLanguageCode).HasColumnName("explanation_language_code").HasMaxLength(35).IsRequired();
            pair.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
            pair.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
            pair.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
            pair.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
            pair.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
            pair.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
            pair.HasIndex(x => new { x.PortfolioId, x.NativeLanguageCode, x.TargetLanguageCode })
                .IsUnique()
                .HasFilter("\"status\" <> 'Archived'");
            pair.HasIndex(x => new { x.PortfolioId, x.Status })
                .IsUnique()
                .HasFilter("\"status\" = 'Active'");
        });
    }
}
