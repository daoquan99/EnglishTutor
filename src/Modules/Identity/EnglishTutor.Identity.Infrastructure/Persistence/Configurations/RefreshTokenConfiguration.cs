using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(t => t.Id);
        b.Property(t => t.UserId).IsRequired();
        b.Property(t => t.FamilyId).IsRequired();
        b.Property(t => t.TokenHash).HasMaxLength(200).IsRequired();
        b.HasIndex(t => t.TokenHash).IsUnique();
        b.Property(t => t.ExpiresAtUtc).IsRequired();
        // Property UsedAtUtc is mapped to column "consumed_at_utc" (Strategy A: clean first migration).
        b.Property(t => t.UsedAtUtc).HasColumnName("consumed_at_utc");
        b.Property(t => t.RevokedAtUtc);
        b.Property(t => t.ReplacedByTokenId);
        b.Property(t => t.CreatedByIp).HasMaxLength(64);
        b.Property(t => t.ReuseDetectedAtUtc);

        b.HasIndex(t => new { t.FamilyId, t.UsedAtUtc });
        b.HasIndex(t => new { t.UserId, t.ExpiresAtUtc });

        b.Ignore(t => t.DomainEvents);
    }
}
