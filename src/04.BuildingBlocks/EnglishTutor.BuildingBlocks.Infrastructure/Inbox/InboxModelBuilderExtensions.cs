using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Inbox;

public static class InboxModelBuilderExtensions
{
    public static void AddNativeInbox(this ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<InboxMessage>();
        entity.HasKey(message => new { message.MessageId, message.ConsumerName });
        entity.Property(message => message.ConsumerName).HasMaxLength(200);
        entity.Property(message => message.ContractName).HasMaxLength(200);
        entity.Property(message => message.SchemaVersion).HasMaxLength(32);
        entity.HasIndex(message => message.CompletedAtUtc);
    }
}
