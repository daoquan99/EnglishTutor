using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Progress.Infrastructure.Persistence;

public sealed class ProgressDbContext(DbContextOptions<ProgressDbContext> options)
    : DbContext(options), IProgressUnitOfWork
{
    public DbSet<LearningActivityLog> LearningActivityLogs => Set<LearningActivityLog>();
    public DbSet<UserExperience> UserExperiences => Set<UserExperience>();
    public DbSet<ExperienceTransaction> ExperienceTransactions => Set<ExperienceTransaction>();
    public DbSet<UserSkillProgress> UserSkillProgresses => Set<UserSkillProgress>();
    public DbSet<UserDailyProgress> UserDailyProgresses => Set<UserDailyProgress>();
    public DbSet<UserWeeklyProgress> UserWeeklyProgresses => Set<UserWeeklyProgress>();
    public DbSet<UserMonthlyProgress> UserMonthlyProgresses => Set<UserMonthlyProgress>();
    public DbSet<UserDashboardSnapshot> UserDashboardSnapshots => Set<UserDashboardSnapshot>();
    public DbSet<UserStreak> UserStreaks => Set<UserStreak>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("progress");

        modelBuilder.Entity<LearningActivityLog>(builder =>
        {
            builder.ToTable("LearningActivityLogs");
            builder.HasKey(log => log.Id);
            builder.Property(log => log.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(log => log.ActivityType).HasConversion<string>().HasMaxLength(100);
            builder.HasIndex(log => new { log.UserId, log.TargetLanguageCode, log.CompletedAtUtc });
        });

        modelBuilder.Entity<UserExperience>(builder =>
        {
            builder.ToTable("UserExperiences");
            builder.HasKey(exp => exp.Id);
            builder.Property(exp => exp.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(exp => exp.CurrentAppRank).HasConversion<string>().HasMaxLength(50);
            builder.HasIndex(exp => new { exp.UserId, exp.TargetLanguageCode }).IsUnique();
            builder.Metadata.FindNavigation(nameof(UserExperience.Transactions))!.SetPropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ExperienceTransaction>(builder =>
        {
            builder.ToTable("ExperienceTransactions");
            builder.HasKey(transaction => transaction.Id);
        });

        modelBuilder.Entity<UserSkillProgress>(builder =>
        {
            builder.ToTable("UserSkillProgresses");
            builder.HasKey(progress => progress.Id);
            builder.Property(progress => progress.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.Property(progress => progress.Skill).HasConversion<string>().HasMaxLength(50);
            builder.HasIndex(progress => new { progress.UserId, progress.TargetLanguageCode, progress.Skill }).IsUnique();
        });

        modelBuilder.Entity<UserDailyProgress>(builder =>
        {
            builder.ToTable("UserDailyProgresses");
            builder.HasKey(progress => progress.Id);
            builder.Property(progress => progress.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(progress => new { progress.UserId, progress.TargetLanguageCode, progress.Date }).IsUnique();
        });

        modelBuilder.Entity<UserWeeklyProgress>(builder =>
        {
            builder.ToTable("UserWeeklyProgresses");
            builder.HasKey(progress => progress.Id);
            builder.Property(progress => progress.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(progress => new { progress.UserId, progress.TargetLanguageCode, progress.Year, progress.WeekNumber }).IsUnique();
        });

        modelBuilder.Entity<UserMonthlyProgress>(builder =>
        {
            builder.ToTable("UserMonthlyProgresses");
            builder.HasKey(progress => progress.Id);
            builder.Property(progress => progress.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(progress => new { progress.UserId, progress.TargetLanguageCode, progress.Year, progress.Month }).IsUnique();
        });

        modelBuilder.Entity<UserDashboardSnapshot>(builder =>
        {
            builder.ToTable("UserDashboardSnapshots");
            builder.HasKey(snapshot => snapshot.Id);
            builder.Property(snapshot => snapshot.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(snapshot => new { snapshot.UserId, snapshot.TargetLanguageCode, snapshot.Date }).IsUnique();
        });

        modelBuilder.Entity<UserStreak>(builder =>
        {
            builder.ToTable("UserStreaks");
            builder.HasKey(streak => streak.Id);
            builder.Property(streak => streak.TargetLanguageCode)
                .HasConversion(code => code.Value, value => BuildingBlocks.SharedKernel.LanguageCode.Create(value))
                .HasMaxLength(3);
            builder.HasIndex(streak => new { streak.UserId, streak.TargetLanguageCode }).IsUnique();
        });

        modelBuilder.Entity<InboxMessage>(builder =>
        {
            builder.ToTable("InboxMessages");
            builder.HasKey(message => message.Id);
            builder.Property(message => message.EventType).HasMaxLength(1000);
            builder.Property(message => message.HandlerName).HasMaxLength(300);
            builder.HasIndex(message => new { message.EventId, message.HandlerName }).IsUnique();
        });
    }
}
