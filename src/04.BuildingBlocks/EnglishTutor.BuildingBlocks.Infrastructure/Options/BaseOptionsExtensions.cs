using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Infrastructure.DateTime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Options;

/// <summary>
/// Registers and validates the strongly-typed infrastructure options classes
/// (database connection string, RabbitMQ, Redis) from configuration.
///
/// Validation runs on startup so the host fails fast when required config is
/// missing or invalid.
/// </summary>
public static class BaseOptionsExtensions
{
    /// <summary>
    /// Binds and validates all infrastructure option classes at startup.
    /// Call once from each host's composition root.
    /// </summary>
    public static IServiceCollection AddBaseOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<DatabaseStartupOptions>()
            .Bind(configuration.GetSection(DatabaseStartupOptions.SectionName))
            .ValidateOnStart();

        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<RedisOptions>()
            .Bind(configuration.GetSection(RedisOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Shared infrastructure primitives used by every host.
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        return services;
    }
}
