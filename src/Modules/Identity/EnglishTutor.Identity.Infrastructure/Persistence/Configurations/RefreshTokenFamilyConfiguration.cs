using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenFamilyConfiguration : IEntityTypeConfiguration<RefreshTokenFamily>
{
    public void Configure(EntityTypeBuilder<RefreshTokenFamily> b)
    {
        b.ToTable("refresh_token_families");
        b.HasKey(f => f.Id);
        b.Property(f => f.UserId).IsRequired();
        b.Property(f => f.SessionId).IsRequired();
        b.Property(f => f.CreatedAtUtc).IsRequired();
        b.Property(f => f.RevokedAtUtc);
        b.Property(f => f.RevokedReason).HasMaxLength(200);

        // 1:N to RefreshToken (child entity, owned by family).
        b.HasMany(f => f.Tokens)
            .WithOne()
            .HasForeignKey(t => t.FamilyId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(f => new { f.SessionId });
        b.HasIndex(f => new { f.UserId, f.SessionId });
        b.HasIndex(f => f.RevokedAtUtc);
    }
}
