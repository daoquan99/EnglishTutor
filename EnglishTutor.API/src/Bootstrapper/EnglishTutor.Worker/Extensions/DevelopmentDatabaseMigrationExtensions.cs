using EnglishTutor.Modules.AdminReports.Infrastructure.Persistence;
using EnglishTutor.Modules.AI.Infrastructure.Persistence;
using EnglishTutor.Modules.Assessments.Infrastructure.Persistence;
using EnglishTutor.Modules.Auth.Infrastructure.Persistence;
using EnglishTutor.Modules.Exercises.Infrastructure.Persistence;
using EnglishTutor.Modules.LearningContent.Infrastructure.Persistence;
using EnglishTutor.Modules.Mistakes.Infrastructure.Persistence;
using EnglishTutor.Modules.Notifications.Infrastructure.Persistence;
using EnglishTutor.Modules.Progress.Infrastructure.Persistence;
using EnglishTutor.Modules.Speaking.Infrastructure.Persistence;
using EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence;
using EnglishTutor.Worker.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Worker.Extensions;

public static class DevelopmentDatabaseMigrationExtensions
{
    public static async Task MigrateWorkerDatabasesAsync(this IHost host)
    {
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var environment = host.Services.GetRequiredService<IHostEnvironment>();
        var autoMigrate = configuration.GetValue("Database:AutoMigrate", environment.IsDevelopment());
        var validateMigrations = configuration.GetValue("Database:ValidateMigrations", true);

        await using var scope = host.Services.CreateAsyncScope();

        if (!autoMigrate)
        {
            if (validateMigrations)
            {
                await EnsureNoPendingMigrationsAsync(scope.ServiceProvider);
            }

            return;
        }

        await MigrateAsync<AuthDbContext>(scope.ServiceProvider);
        await MigrateAsync<UsersDbContext>(scope.ServiceProvider);
        await MigrateAsync<AiDbContext>(scope.ServiceProvider);
        await MigrateAsync<VocabularyDbContext>(scope.ServiceProvider);
        await MigrateAsync<SpeakingDbContext>(scope.ServiceProvider);
        await MigrateAsync<MistakesDbContext>(scope.ServiceProvider);
        await MigrateAsync<ProgressDbContext>(scope.ServiceProvider);
        await MigrateAsync<StudyPlansDbContext>(scope.ServiceProvider);
        await MigrateAsync<NotificationsDbContext>(scope.ServiceProvider);
        await MigrateAsync<LearningContentDbContext>(scope.ServiceProvider);
        await MigrateAsync<ExercisesDbContext>(scope.ServiceProvider);
        await MigrateAsync<AssessmentsDbContext>(scope.ServiceProvider);
        await MigrateAsync<AdminReportsDbContext>(scope.ServiceProvider);
        await MigrateAsync<MessagingDbContext>(scope.ServiceProvider);

        if (validateMigrations)
        {
            await EnsureNoPendingMigrationsAsync(scope.ServiceProvider);
        }
    }

    private static async Task MigrateAsync<TDbContext>(IServiceProvider services)
        where TDbContext : DbContext
    {
        var dbContext = services.GetRequiredService<TDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    private static async Task EnsureNoPendingMigrationsAsync(IServiceProvider services)
    {
        await EnsureNoPendingMigrationsAsync<AuthDbContext>(services);
        await EnsureNoPendingMigrationsAsync<UsersDbContext>(services);
        await EnsureNoPendingMigrationsAsync<AiDbContext>(services);
        await EnsureNoPendingMigrationsAsync<VocabularyDbContext>(services);
        await EnsureNoPendingMigrationsAsync<SpeakingDbContext>(services);
        await EnsureNoPendingMigrationsAsync<MistakesDbContext>(services);
        await EnsureNoPendingMigrationsAsync<ProgressDbContext>(services);
        await EnsureNoPendingMigrationsAsync<StudyPlansDbContext>(services);
        await EnsureNoPendingMigrationsAsync<NotificationsDbContext>(services);
        await EnsureNoPendingMigrationsAsync<LearningContentDbContext>(services);
        await EnsureNoPendingMigrationsAsync<ExercisesDbContext>(services);
        await EnsureNoPendingMigrationsAsync<AssessmentsDbContext>(services);
        await EnsureNoPendingMigrationsAsync<AdminReportsDbContext>(services);
        await EnsureNoPendingMigrationsAsync<MessagingDbContext>(services);
    }

    private static async Task EnsureNoPendingMigrationsAsync<TDbContext>(IServiceProvider services)
        where TDbContext : DbContext
    {
        var dbContext = services.GetRequiredService<TDbContext>();
        var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToArray();
        if (pendingMigrations.Length > 0)
        {
            throw new InvalidOperationException(
                $"{typeof(TDbContext).Name} has pending migrations: {string.Join(", ", pendingMigrations)}. " +
                "Apply migrations before starting the Worker or enable Database:AutoMigrate for local development.");
        }
    }
}
