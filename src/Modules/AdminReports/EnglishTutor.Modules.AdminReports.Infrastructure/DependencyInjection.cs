using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.AdminReports.Application.Abstractions;
using EnglishTutor.Modules.AdminReports.Application.EventHandlers;
using EnglishTutor.Modules.AdminReports.Infrastructure.Persistence;
using EnglishTutor.Modules.AdminReports.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.AdminReports.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAdminReportsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AdminReportsDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "adminreports"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IAdminReportQueryService, AdminReportQueryService>();
        services.AddScoped<IAdminReportProjectionRepository, AdminReportProjectionRepository>();
        services.AddScoped<IAdminAuditLogRepository, AdminAuditLogRepository>();
        services.AddScoped<IAdminReportsInboxStore, AdminReportsInboxStore>();
        services.AddScoped<IAdminReportsUnitOfWork>(provider => provider.GetRequiredService<AdminReportsDbContext>());
        services.AddScoped<IIntegrationEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredEventHandler>();
        services.AddScoped<IIntegrationEventHandler<UserLevelChangedIntegrationEvent>, UserLevelChangedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<SpeakingSessionCompletedIntegrationEvent>, SpeakingSessionCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ExerciseCompletedIntegrationEvent>, ExerciseCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<VocabularyMasteredIntegrationEvent>, VocabularyMasteredEventHandler>();

        return services;
    }
}
