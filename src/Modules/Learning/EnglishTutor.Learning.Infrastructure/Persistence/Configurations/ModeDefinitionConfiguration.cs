using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Configurations;

internal sealed class ModeDefinitionConfiguration : IEntityTypeConfiguration<ModeDefinition>
{
    public void Configure(EntityTypeBuilder<ModeDefinition> b)
    {
        b.ToTable("mode_definitions");
        b.HasKey(m => m.Id);

        b.Property(m => m.Id).HasColumnName("id");
        b.Property(m => m.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        b.Property(m => m.Description).HasColumnName("description");
        b.Property(m => m.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        b.OwnsOne(m => m.Code, c =>
        {
            c.Property(code => code.Value).HasColumnName("code").HasMaxLength(50).IsRequired();
            c.WithOwner();
            c.HasIndex(code => code.Value).HasDatabaseName("ix_mode_definitions_code").IsUnique().HasFilter("is_deleted = false");
        });
        b.Navigation(m => m.Code).IsRequired();

        // Audit columns:
        b.Property(m => m.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(m => m.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(m => m.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(m => m.UpdatedByUserId).HasColumnName("updated_by_user_id");
        
        // Soft delete columns:
        b.Property(m => m.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        b.Property(m => m.DeletedAtUtc).HasColumnName("deleted_at_utc");
        b.Property(m => m.DeletedByUserId).HasColumnName("deleted_by_user_id");

        b.Ignore(m => m.DomainEvents);
    }
}
