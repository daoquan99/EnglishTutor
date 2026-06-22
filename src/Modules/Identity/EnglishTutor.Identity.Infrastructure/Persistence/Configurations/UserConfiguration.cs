using EnglishTutor.Identity.Domain.Aggregates.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);

        b.Property(u => u.DisplayName).HasMaxLength(200).IsRequired();

        // Email as owned value object.
        b.OwnsOne(u => u.Email, e =>
        {
            e.Property(email => email.Value).HasColumnName("email").HasMaxLength(320).IsRequired();
            e.WithOwner();
            e.HasIndex(email => email.Value).IsUnique();
        });
        b.Navigation(u => u.Email).HasField("_emailBacking");

        // PasswordHash as owned value object (never queryable in plaintext).
        b.OwnsOne(u => u.PasswordHash, p =>
        {
            p.Property(h => h.Hash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            p.WithOwner();
        });
        b.Navigation(u => u.PasswordHash).HasField("_passwordHashBacking");

        b.Property(u => u.IsActive).IsRequired();
        b.Property(u => u.IsLockedOut).IsRequired();
        b.Property(u => u.LockoutEndUtc);
        b.Property(u => u.FailedLoginAttempts).IsRequired();

        // Domain events are not persisted.
        b.Ignore(u => u.DomainEvents);
        b.Ignore(u => u.RoleIds);
    }
}
