using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Domain.UserOverviewCard;
using EnglishTutor.Modules.AdminReports.Domain.DailyAiUsageReport;
using EnglishTutor.Modules.AdminReports.Domain.LearningActivityReport;
using EnglishTutor.Modules.AdminReports.Domain.CommonMistakeStat;
using EnglishTutor.Modules.AdminReports.Domain.AssessmentPassRateReport;
using EnglishTutor.Modules.AdminReports.Domain.RetentionReport;
using EnglishTutor.Modules.AdminReports.Domain.AuditLog;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.AdminReports.Infrastructure.Persistence;

public sealed class AdminReportsDbContext(DbContextOptions<AdminReportsDbContext> options) : DbContext(options), IAdminReportsUnitOfWork
{
    public DbSet<UserOverviewCard> UserOverviewCards => Set<UserOverviewCard>();
    public DbSet<DailyAiUsageReport> DailyAiUsageReports => Set<DailyAiUsageReport>();
    public DbSet<LearningActivityReport> LearningActivityReports => Set<LearningActivityReport>();
    public DbSet<CommonMistakeStat> CommonMistakeStats => Set<CommonMistakeStat>();
    public DbSet<AssessmentPassRateReport> AssessmentPassRateReports => Set<AssessmentPassRateReport>();
    public DbSet<RetentionReport> RetentionReports => Set<RetentionReport>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<DeadLetterMessage> DeadLetterMessages => Set<DeadLetterMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("adminreports");

        modelBuilder.Entity<UserOverviewCard>(builder =>
        {
            builder.ToTable("UserOverviewCards");
            builder.HasKey(card => card.Id);
            builder.Property(card => card.Email).HasMaxLength(320);
            builder.Property(card => card.DisplayName).HasMaxLength(200);
            builder.Property(card => card.TargetLanguageCode).HasMaxLength(3);
            builder.Property(card => card.CurrentLevel).HasMaxLength(10);
            builder.HasIndex(card => card.UserId).IsUnique();
            builder.HasIndex(card => card.TargetLanguageCode);
            builder.HasIndex(card => card.TotalExp);
            builder.HasIndex(card => card.LastActivityAtUtc);
            builder.HasIndex(card => card.RegisteredAtUtc);
        });

        modelBuilder.Entity<DailyAiUsageReport>(builder =>
        {
            builder.ToTable("DailyAiUsageReports");
            builder.HasKey(report => report.Id);
            builder.Property(report => report.ModelType).HasMaxLength(100);
            builder.Property(report => report.TaskType).HasMaxLength(100);
            builder.Property(report => report.EstimatedCostUsd).HasPrecision(18, 6);
            builder.HasIndex(report => new { report.ReportDate, report.ModelType });
        });

        modelBuilder.Entity<LearningActivityReport>(builder =>
        {
            builder.ToTable("LearningActivityReports");
            builder.HasKey(report => report.Id);
            builder.Property(report => report.Period).HasConversion<string>().HasMaxLength(20);
            builder.HasIndex(report => new { report.ReportDate, report.Period });
        });

        modelBuilder.Entity<CommonMistakeStat>(builder =>
        {
            builder.ToTable("CommonMistakeStats");
            builder.HasKey(stat => stat.Id);
            builder.Property(stat => stat.TargetLanguageCode).HasMaxLength(3);
            builder.Property(stat => stat.MistakeType).HasMaxLength(100);
            builder.Property(stat => stat.Category).HasMaxLength(200);
            builder.Property(stat => stat.ExampleOriginal).HasColumnType("text");
            builder.Property(stat => stat.ExampleCorrected).HasColumnType("text");
            builder.HasIndex(stat => new { stat.TargetLanguageCode, stat.OccurrenceCount });
        });

        modelBuilder.Entity<AssessmentPassRateReport>(builder =>
        {
            builder.ToTable("AssessmentPassRateReports");
            builder.HasKey(report => report.Id);
            builder.Property(report => report.TargetLanguageCode).HasMaxLength(3);
            builder.Property(report => report.AssessmentType).HasMaxLength(50);
            builder.Property(report => report.ForLevel).HasMaxLength(10);
            builder.Property(report => report.PassRate).HasPrecision(9, 4);
            builder.Property(report => report.AverageScore).HasPrecision(9, 2);
            builder.Property(report => report.Period).HasConversion<string>().HasMaxLength(20);
            builder.HasIndex(report => new { report.ReportDate, report.TargetLanguageCode, report.AssessmentType });
        });

        modelBuilder.Entity<RetentionReport>(builder =>
        {
            builder.ToTable("RetentionReports");
            builder.HasKey(report => report.Id);
            builder.Property(report => report.Period).HasConversion<string>().HasMaxLength(20);
            builder.Property(report => report.RetentionRate).HasPrecision(9, 4);
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(log => log.Id);
            builder.Property(log => log.Action).HasConversion<string>().HasMaxLength(100);
            builder.Property(log => log.TargetEntity).HasMaxLength(200);
            builder.Property(log => log.TargetEntityId).HasMaxLength(200);
            builder.Property(log => log.OldValue).HasColumnType("jsonb");
            builder.Property(log => log.NewValue).HasColumnType("jsonb");
            builder.Property(log => log.IpAddress).HasMaxLength(100);
            builder.Property(log => log.UserAgent).HasMaxLength(1000);
            builder.HasIndex(log => new { log.AdminUserId, log.CreatedAtUtc });
            builder.HasIndex(log => new { log.TargetEntity, log.TargetEntityId });
        });

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("InboxMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(1000);
            builder.Property(message => message.HandlerName).HasMaxLength(300);
            builder.HasIndex(message => new { message.EventId, message.HandlerName }).IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(1000).IsRequired();
            builder.Property(message => message.SourceModule).HasMaxLength(100).IsRequired();
            builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(32);
            builder.Property(message => message.Payload).IsRequired();
            builder.HasIndex(message => new { message.Status, message.NextRetryAtUtc });
        });

        modelBuilder.Entity<DeadLetterMessage>(builder =>
        {
            builder.ToTable("DeadLetterMessages", "messaging", table => table.ExcludeFromMigrations());
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(1000).IsRequired();
            builder.Property(message => message.SourceModule).HasMaxLength(100).IsRequired();
            builder.Property(message => message.Status).HasConversion<string>().HasMaxLength(32);
            builder.Property(message => message.Payload).IsRequired();
            builder.Property(message => message.LastError).HasColumnType("text");
            builder.Property(message => message.StackTrace).HasColumnType("text");
            builder.HasIndex(message => new { message.Status, message.FailedAtUtc });
        });
    }
}
