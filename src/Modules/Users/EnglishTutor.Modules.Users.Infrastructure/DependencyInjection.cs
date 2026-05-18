using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.Modules.Auth.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.EventHandlers;
using EnglishTutor.Modules.Users.Contracts.Readers;
using EnglishTutor.Modules.Users.Infrastructure.ContractReaders;
using EnglishTutor.Modules.Users.Infrastructure.Persistence;
using EnglishTutor.Modules.Users.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "users")));

        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IUserLanguageSettingsRepository, UserLanguageSettingsRepository>();
        services.AddScoped<IUserTargetLanguageRepository, UserTargetLanguageRepository>();
        services.AddScoped<IUsersUnitOfWork>(provider => provider.GetRequiredService<UsersDbContext>());
        services.AddScoped<IUsersInboxStore, UsersInboxStore>();

        services.AddScoped<IUserProfileReader, UserProfileReader>();
        services.AddScoped<IUserLanguageSettingsReader, UserLanguageSettingsReader>();
        services.AddScoped<IUserTargetLanguageReader, UserTargetLanguageReader>();
        services.AddScoped<IIntegrationEventHandler<UserRegisteredIntegrationEvent>, UserRegisteredIntegrationEventHandler>();

        return services;
    }
}
