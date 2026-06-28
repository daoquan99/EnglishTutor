using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Outbox;

public static class OutboxModelBuilderExtensions
{
    public static void AddNativeOutbox(this ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<OutboxMessage>();
        entity.HasKey(message => message.Id);
        entity.Property(message => message.ContractName).HasMaxLength(200).IsRequired();
        entity.Property(message => message.SchemaVersion).HasMaxLength(32).IsRequired();
        entity.Property(message => message.ExchangeName).HasMaxLength(120).IsRequired();
        entity.Property(message => message.RoutingKey).HasMaxLength(200).IsRequired();
        entity.Property(message => message.PayloadJson).IsRequired();
        entity.Property(message => message.Status).HasConversion<string>().HasMaxLength(32);
        entity.Property(message => message.LastErrorCode).HasMaxLength(100);
        entity.Property(message => message.LastErrorMessage).HasMaxLength(1000);
        entity.HasIndex(message => new { message.Status, message.NextAttemptAtUtc, message.OccurredAtUtc });
        entity.HasIndex(message => message.LockedUntilUtc);
        entity.HasIndex(message => message.PublishedAtUtc);
    }
}
