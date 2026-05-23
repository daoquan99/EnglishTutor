using EnglishTutor.BuildingBlocks.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Worker.Outbox;

public sealed class MessagingDbContext(DbContextOptions<MessagingDbContext> options) : DbContext(options)
{
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("messaging");

        modelBuilder.Entity<DeadLetterMessage>(builder =>
        {
            builder.ToTable("DeadLetterMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(1000).IsRequired();
            builder.Property(message => message.SourceModule).HasMaxLength(100).IsRequired();
            builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(32);
            builder.Property(message => message.Payload).IsRequired();
            builder.Property(message => message.LastError).HasColumnType("text");
            builder.Property(message => message.StackTrace).HasColumnType("text");
            builder.HasIndex(message => new { message.Status, message.FailedAtUtc });
            builder.HasIndex(message => message.EventId).IsUnique();
        });
    }
}
