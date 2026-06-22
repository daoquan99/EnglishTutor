using EnglishTutor.Identity.Domain.Aggregates.Sessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> b)
    {
        b.ToTable("user_sessions");
        b.HasKey(s => s.Id);
        b.Property(s => s.UserId).IsRequired();
        b.Property(s => s.LastSeenAtUtc).IsRequired();
        b.Property(s => s.RevokedAtUtc);
        b.Property(s => s.RevokedReason).HasMaxLength(200);

        // DeviceInfo as owned value object. No raw PII is persisted.
        b.OwnsOne(s => s.Device, d =>
        {
            d.Property(x => x.DeviceId).HasColumnName("device_id").HasMaxLength(100).IsRequired();
            d.Property(x => x.DeviceName).HasColumnName("device_name").HasMaxLength(200);
            d.Property(x => x.UserAgentHash).HasColumnName("user_agent_hash").HasMaxLength(64).IsRequired();
            d.Property(x => x.IpAddressHash).HasColumnName("ip_address_hash").HasMaxLength(64);
        });

        // 1:1 to RefreshTokenFamily (optional nav). Family may be missing
        // if the session is loaded without its family.
        b.HasOne(s => s.Family)
            .WithOne()
            .HasForeignKey<RefreshTokenFamily>(f => f.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(s => s.UserId);
        b.HasIndex(s => s.RevokedAtUtc);

        b.Ignore(s => s.DomainEvents);
    }
}
