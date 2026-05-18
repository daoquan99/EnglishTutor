using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.EventBus.InProcess;
using EnglishTutor.BuildingBlocks.Outbox;
using EnglishTutor.Modules.AI.Infrastructure;
using EnglishTutor.Modules.Auth.Infrastructure;
using EnglishTutor.Modules.Mistakes.Infrastructure;
using EnglishTutor.Modules.Progress.Infrastructure;
using EnglishTutor.Modules.Speaking.Infrastructure;
using EnglishTutor.Modules.Users.Infrastructure;
using EnglishTutor.Modules.Vocabulary.Infrastructure;
using EnglishTutor.Worker.Outbox;
using EnglishTutor.Worker.Jobs;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace EnglishTutor.Worker;

public static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IEventBus, InProcessEventBus>();
        services
            .AddAuthModule(configuration)
            .AddUsersModule(configuration)
            .AddAiModule(configuration)
            .AddVocabularyModule(configuration)
            .AddSpeakingModule(configuration)
            .AddMistakesModule(configuration)
            .AddProgressModule(configuration);

        services.AddDbContext<MessagingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "messaging")));
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
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
