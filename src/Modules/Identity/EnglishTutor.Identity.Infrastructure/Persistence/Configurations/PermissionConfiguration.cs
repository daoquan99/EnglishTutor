using EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Configurations;

internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> b)
    {
        b.ToTable("permissions");
        b.HasKey(p => p.Id);
        b.Property(p => p.Code).HasMaxLength(200).IsRequired();
        b.HasIndex(p => p.Code).IsUnique();
        b.Property(p => p.ModuleName).HasMaxLength(100).IsRequired();
        b.Property(p => p.DisplayName).HasMaxLength(200).IsRequired();
    }
}
