using EnglishTutor.Audit.Domain.SecurityEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Audit.Infrastructure.Persistence.Configurations;

internal sealed class SecurityEventConfiguration : IEntityTypeConfiguration<SecurityEvent>
{
    public void Configure(EntityTypeBuilder<SecurityEvent> b)
    {
        b.ToTable("security_events", schema: "audit");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasColumnName("Id");

        b.Property(e => e.CategoryCode).HasMaxLength(100).IsRequired();
        b.Property(e => e.SourceModule).HasMaxLength(100).IsRequired();
        b.Property(e => e.SourceEventType).HasMaxLength(200).IsRequired();

        b.Property(e => e.ReasonCode).HasMaxLength(100);
        b.Property(e => e.IpAddressHash).HasMaxLength(64);
        b.Property(e => e.UserAgentHash).HasMaxLength(64);

        b.Property(e => e.OccurredAtUtc).IsRequired();

        b.HasIndex(e => new { e.CategoryCode, e.OccurredAtUtc });
        b.HasIndex(e => new { e.UserId, e.OccurredAtUtc });
    }
}
