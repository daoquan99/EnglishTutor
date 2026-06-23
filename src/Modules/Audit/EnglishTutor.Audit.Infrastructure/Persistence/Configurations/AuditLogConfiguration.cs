using EnglishTutor.Audit.Domain.Aggregates.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Audit.Infrastructure.Persistence.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_logs", schema: "audit");
        b.HasKey(l => l.Id);

        b.Property(l => l.Id)
            .HasColumnName("id");

        b.Property(l => l.UserId)
            .HasColumnName("user_id");

        b.Property(l => l.Action)
            .HasColumnName("action")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(l => l.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(200)
            .IsRequired();

        b.Property(l => l.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(100)
            .IsRequired();

        b.Property(l => l.DetailJson)
            .HasColumnName("detail_json")
            .HasColumnType("jsonb")
            .IsRequired();

        b.Property(l => l.IpAddressHash)
            .HasColumnName("ip_address_hash")
            .HasMaxLength(64);

        b.Property(l => l.UserAgentHash)
            .HasColumnName("user_agent_hash")
            .HasMaxLength(64);

        b.Property(l => l.CorrelationId)
            .HasColumnName("correlation_id");

        b.Property(l => l.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired();

        b.Ignore(l => l.CreatedByUserId);
        b.Ignore(l => l.UpdatedAtUtc);
        b.Ignore(l => l.UpdatedByUserId);

        // Indexes
        b.HasIndex(l => new { l.EntityType, l.EntityId, l.CreatedAtUtc })
            .HasDatabaseName("IX_audit_logs_entity_type_entity_id_created_at_utc");

        b.HasIndex(l => new { l.UserId, l.CreatedAtUtc })
            .HasDatabaseName("IX_audit_logs_user_id_created_at_utc");

        b.HasIndex(l => l.CreatedAtUtc)
            .HasDatabaseName("IX_audit_logs_created_at_utc");
    }
}
