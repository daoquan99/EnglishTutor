using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.EventBus.InProcess;
using EnglishTutor.BuildingBlocks.Infrastructure;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Storage;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.AdminReports.Infrastructure;
using EnglishTutor.Modules.AdminReports.Infrastructure.Persistence;
using EnglishTutor.Modules.AI.Infrastructure;
using EnglishTutor.Modules.Assessments.Infrastructure;
using EnglishTutor.Modules.Assessments.Infrastructure.Persistence;
using EnglishTutor.Modules.Auth.Infrastructure;
using EnglishTutor.Modules.Auth.Infrastructure.Persistence;
using EnglishTutor.Modules.Exercises.Infrastructure;
using EnglishTutor.Modules.Exercises.Infrastructure.Persistence;
using EnglishTutor.Modules.LearningContent.Infrastructure;
using EnglishTutor.Modules.LearningContent.Infrastructure.Persistence;
using EnglishTutor.Modules.Mistakes.Infrastructure;
using EnglishTutor.Modules.Mistakes.Infrastructure.Persistence;
using EnglishTutor.Modules.Notifications.Infrastructure;
using EnglishTutor.Modules.Progress.Infrastructure;
using EnglishTutor.Modules.Speaking.Infrastructure;
using EnglishTutor.Modules.Speaking.Infrastructure.Persistence;
using EnglishTutor.Modules.StudyPlans.Infrastructure;
using EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence;
using EnglishTutor.Modules.Users.Infrastructure;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using EnglishTutor.Modules.Vocabulary.Infrastructure;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence;
using EnglishTutor.Worker.Outbox;
using EnglishTutor.Worker.Jobs;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace EnglishTutor.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEventBus, InProcessEventBus>();
        services.AddScoped<ICurrentUser, SystemCurrentUser>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.Configure<OutboxOptions>(configuration.GetSection("Outbox"));
        services.AddFileStorage(configuration);
        services
            .AddAuthModule(configuration)
            .AddUsersModule(configuration)
            .AddAiModule(configuration)
            .AddVocabularyModule(configuration)
            .AddSpeakingModule(configuration)
            .AddMistakesModule(configuration)
            .AddProgressModule(configuration)
            .AddStudyPlansModule(configuration)
            .AddNotificationsModule(configuration)
            .AddLearningContentModule(configuration)
            .AddExercisesModule(configuration)
            .AddAssessmentsModule(configuration)
            .AddAdminReportsModule(configuration);

        services.AddDbContext<MessagingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "messaging")));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("auth", provider.GetRequiredService<AuthDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("users", provider.GetRequiredService<UsersDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("vocabulary", provider.GetRequiredService<VocabularyDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("speaking", provider.GetRequiredService<SpeakingDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("mistakes", provider.GetRequiredService<MistakesDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("studyplans", provider.GetRequiredService<StudyPlansDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("learningcontent", provider.GetRequiredService<LearningContentDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("exercises", provider.GetRequiredService<ExercisesDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("assessments", provider.GetRequiredService<AssessmentsDbContext>()));
        services.AddScoped<IModuleOutboxStore>(provider => new EfCoreModuleOutboxStore("adminreports", provider.GetRequiredService<AdminReportsDbContext>()));
        services.AddScoped<OutboxMessageDispatcher>();
        services.AddScoped<IOutboxProcessor, EfCoreOutboxProcessor>();

        services.AddQuartz(q =>
        {
            var outboxJobKey = new JobKey("OutboxProcessingJob");
            q.AddJob<OutboxProcessingJob>(opts => opts.WithIdentity(outboxJobKey));
            q.AddTrigger(opts => opts
                .ForJob(outboxJobKey)
                .WithIdentity("OutboxProcessingTrigger")
                .WithSimpleSchedule(x => x
                    .WithIntervalInSeconds(5)
                    .RepeatForever()));

            var missedSessionJobKey = new JobKey("MissedSessionDetectionJob");
            q.AddJob<MissedSessionDetectionJob>(opts => opts.WithIdentity(missedSessionJobKey));
            q.AddTrigger(opts => opts
                .ForJob(missedSessionJobKey)
                .WithIdentity("MissedSessionDetectionTrigger")
                .WithCronSchedule("0 0/15 * * * ?"));

            var studyReminderJobKey = new JobKey("StudyReminderSchedulingJob");
            q.AddJob<StudyReminderSchedulingJob>(opts => opts.WithIdentity(studyReminderJobKey));
            q.AddTrigger(opts => opts
                .ForJob(studyReminderJobKey)
                .WithIdentity("StudyReminderSchedulingTrigger")
                .WithCronSchedule("0 0/5 * * * ?"));

            var weeklyReportJobKey = new JobKey("WeeklyReportGenerationJob");
            q.AddJob<WeeklyReportGenerationJob>(opts => opts.WithIdentity(weeklyReportJobKey));
            q.AddTrigger(opts => opts
                .ForJob(weeklyReportJobKey)
                .WithIdentity("WeeklyReportGenerationTrigger")
                .WithCronSchedule("0 0 3 ? * MON *"));

            var monthlyReportJobKey = new JobKey("MonthlyReportGenerationJob");
            q.AddJob<MonthlyReportGenerationJob>(opts => opts.WithIdentity(monthlyReportJobKey));
            q.AddTrigger(opts => opts
                .ForJob(monthlyReportJobKey)
                .WithIdentity("MonthlyReportGenerationTrigger")
                .WithCronSchedule("0 0 3 1 * ? *"));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
