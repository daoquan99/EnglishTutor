using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Configurations;

internal sealed class LanguageDefinitionConfiguration : IEntityTypeConfiguration<LanguageDefinition>
{
    public void Configure(EntityTypeBuilder<LanguageDefinition> builder)
    {
        builder.ToTable("language_definitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.Code).HasColumnName("code").HasMaxLength(35).IsRequired();
        builder.Property(x => x.EnglishName).HasColumnName("english_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.NativeName).HasColumnName("native_name").HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(x => x.IsAvailableAsNative).HasColumnName("is_available_as_native").IsRequired();
        builder.Property(x => x.IsAvailableAsTarget).HasColumnName("is_available_as_target").IsRequired();
        builder.Property(x => x.SortOrder).HasColumnName("sort_order").IsRequired();
        builder.Property(x => x.Version).HasColumnName("version").IsConcurrencyToken();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.SortOrder, x.EnglishName, x.Id });
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        builder.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        builder.Property(x => x.UpdatedByUserId).HasColumnName("updated_by_user_id");
        builder.Property(x => x.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(x => x.DeletedAtUtc).HasColumnName("deleted_at_utc");
        builder.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id");
        builder.Ignore(x => x.DomainEvents);
    }
}
