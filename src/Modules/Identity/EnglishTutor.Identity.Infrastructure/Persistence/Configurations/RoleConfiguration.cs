using EnglishTutor.Identity.Domain.Aggregates.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        b.ToTable("roles");
        b.HasKey(r => r.Id);
        b.Property(r => r.Name).HasMaxLength(50).IsRequired();
        b.HasIndex(r => r.Name).IsUnique();
        b.Property(r => r.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(r => r.Priority).IsRequired();
    }
}
