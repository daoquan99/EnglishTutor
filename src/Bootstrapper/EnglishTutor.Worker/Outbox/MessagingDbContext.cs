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
            builder.Property(message => message.EventType).HasMaxLength(1000);
            builder.Property(message => message.SourceModule).HasMaxLength(100);
            builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(32);
        });
    }
}
