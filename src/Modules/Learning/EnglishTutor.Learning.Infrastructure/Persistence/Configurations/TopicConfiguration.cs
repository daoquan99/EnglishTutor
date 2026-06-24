using EnglishTutor.Learning.Domain.Aggregates.Topics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Learning.Infrastructure.Persistence.Configurations;

internal sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> b)
    {
        b.ToTable("topics");
        b.HasKey(t => t.Id);

        b.Property(t => t.Id).HasColumnName("id");
        b.Property(t => t.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        b.Property(t => t.Description).HasColumnName("description");
        b.Property(t => t.IsActive).HasColumnName("is_active").HasDefaultValue(true);

        b.OwnsOne(t => t.Slug, s =>
        {
            s.Property(slug => slug.Value).HasColumnName("slug").HasMaxLength(255).IsRequired();
            s.WithOwner();
            s.HasIndex(slug => slug.Value).HasDatabaseName("ix_topics_slug").IsUnique().HasFilter("is_deleted = false");
        });
        b.Navigation(t => t.Slug).IsRequired();

        // Audit columns:
        b.Property(t => t.CreatedAtUtc).HasColumnName("created_at_utc").IsRequired();
        b.Property(t => t.CreatedByUserId).HasColumnName("created_by_user_id");
        b.Property(t => t.UpdatedAtUtc).HasColumnName("updated_at_utc").IsRequired();
        b.Property(t => t.UpdatedByUserId).HasColumnName("updated_by_user_id");
        
        // Soft delete columns:
        b.Property(t => t.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        b.Property(t => t.DeletedAtUtc).HasColumnName("deleted_at_utc");
        b.Property(t => t.DeletedByUserId).HasColumnName("deleted_by_user_id");

        b.Ignore(t => t.DomainEvents);

        // TopicModes child collection:
        b.HasMany(t => t.TopicModes)
            .WithOne()
            .HasForeignKey(tm => tm.TopicId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
