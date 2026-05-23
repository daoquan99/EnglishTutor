using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
using EnglishTutor.Modules.AdminReports.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Notifications.Application.Abstractions;
using EnglishTutor.Modules.Notifications.Application.EventHandlers;
using EnglishTutor.Modules.Notifications.Infrastructure.Persistence;
using EnglishTutor.Modules.Notifications.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.Notifications.Infrastructure.Seed;
using EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Notifications.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<NotificationsDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "notifications"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationSettingRepository, NotificationSettingRepository>();
        services.AddScoped<IUserNotificationScheduleRepository, UserNotificationScheduleRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationsInboxStore, NotificationsInboxStore>();
        services.AddScoped<INotificationsUnitOfWork>(provider => provider.GetRequiredService<NotificationsDbContext>());
        services.AddScoped<NotificationsDataSeeder>();
        services.AddScoped<IModuleSeeder, NotificationsModuleSeeder>();

        services.AddScoped<IIntegrationEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredEventHandler>();
        services.AddScoped<IIntegrationEventHandler<PlannedStudySessionMissedIntegrationEvent>, PlannedStudySessionMissedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<DailyStudyTargetCompletedIntegrationEvent>, DailyStudyTargetCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<UserLevelChangedIntegrationEvent>, LevelUpEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ProgressSummaryReadyIntegrationEvent>, ProgressSummaryReadyEventHandler>();

        return services;
    }
}
