using EnglishTutor.BuildingBlocks.Domain;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage;
using EnglishTutor.Modules.Notifications.Domain.NotificationMessage.Entities;
using EnglishTutor.Modules.Notifications.Domain.NotificationSetting;
using EnglishTutor.Modules.Notifications.Domain.NotificationTemplate;
using EnglishTutor.Modules.Notifications.Domain.UserNotificationSchedule;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Notifications.Infrastructure.Persistence;

public sealed class NotificationsDbContext(DbContextOptions<NotificationsDbContext> options)
    : DbContext(options), INotificationsUnitOfWork
{
    public DbSet<NotificationSetting> NotificationSettings => Set<NotificationSetting>();
    public DbSet<UserNotificationSchedule> UserNotificationSchedules => Set<UserNotificationSchedule>();
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<NotificationDeliveryLog> NotificationDeliveryLogs => Set<NotificationDeliveryLog>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("notifications");
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<NotificationSetting>(builder =>
        {
            builder.ToTable("NotificationSettings");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.TimeZone).HasMaxLength(100).IsRequired();
            builder.HasIndex(s => s.UserId).IsUnique();
        });

        modelBuilder.Entity<UserNotificationSchedule>(builder =>
        {
            builder.ToTable("UserNotificationSchedules");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.NotificationType).HasConversion<string>().HasMaxLength(50);
            builder.Property(s => s.TargetLanguageCode).HasMaxLength(10);
            builder.Property(s => s.Frequency).HasConversion<string>().HasMaxLength(20);
            builder.HasIndex(s => new { s.UserId, s.NotificationType, s.TargetLanguageCode }).IsUnique();
        });

        modelBuilder.Entity<NotificationMessage>(builder =>
        {
            builder.ToTable("NotificationMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.Type).HasConversion<string>().HasMaxLength(80);
            builder.Property(message => message.Title).HasMaxLength(200).IsRequired();
            builder.Property(message => message.Body).HasMaxLength(2000).IsRequired();
            builder.Property(message => message.Data).HasColumnType("jsonb");
            builder.Property(message => message.Channel).HasConversion<string>().HasMaxLength(30);
            builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(30);
            builder.Property(message => message.LastErrorMessage).HasMaxLength(1000);
            builder.HasIndex(message => new { message.UserId, message.IsRead, message.ScheduledAtUtc });
            builder.HasIndex(message => message.Status);
            builder.Metadata.FindNavigation(nameof(NotificationMessage.DeliveryLogs))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<NotificationDeliveryLog>(builder =>
        {
            builder.ToTable("NotificationDeliveryLogs");
            builder.HasKey(log => log.Id);
            builder.Property(log => log.Channel).HasConversion<string>().HasMaxLength(30);
            builder.Property(log => log.Status).HasConversion<string>().HasMaxLength(30);
            builder.Property(log => log.ErrorMessage).HasMaxLength(1000);
            builder.HasOne<NotificationMessage>()
                .WithMany(message => message.DeliveryLogs)
                .HasForeignKey(log => log.NotificationMessageId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasIndex(log => log.NotificationMessageId);
        });

        modelBuilder.Entity<NotificationTemplate>(builder =>
        {
            builder.ToTable("NotificationTemplates");
            builder.HasKey(template => template.Id);
            builder.Property(template => template.Type).HasConversion<string>().HasMaxLength(80);
            builder.Property(template => template.LanguageCode).HasMaxLength(3).IsRequired();
            builder.Property(template => template.TitleTemplate).HasMaxLength(300).IsRequired();
            builder.Property(template => template.BodyTemplate).HasMaxLength(2000).IsRequired();
            builder.HasIndex(template => new { template.Type, template.LanguageCode })
                .IsUnique()
                .HasFilter("\"IsActive\" = true AND \"IsDeleted\" = false");
        });

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("InboxMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(1000).IsRequired();
            builder.Property(message => message.HandlerName).HasMaxLength(300).IsRequired();
            builder.HasIndex(message => new { message.EventId, message.HandlerName }).IsUnique();
        });

        modelBuilder.ApplySoftDeleteQueryFilters();
    }
}
