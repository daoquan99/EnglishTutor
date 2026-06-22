using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// DI helpers for the auditable-entity infrastructure.
/// </summary>
public static class AuditableEntityServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="AuditableEntitySaveChangesInterceptor"/> in DI.
    /// Each module's DbContext registration should then call
    /// <see cref="AddAuditableEntityInterceptor"/> on its
    /// <see cref="DbContextOptionsBuilder"/> to attach it.
    /// </summary>
    public static IServiceCollection AddAuditableEntityInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        return services;
    }

    /// <summary>
    /// Attaches the scoped interceptor instance to the DbContext options.
    /// Resolve from the application's root service provider.
    /// </summary>
    public static DbContextOptionsBuilder AddAuditableEntityInterceptor(
        this DbContextOptionsBuilder builder,
        IServiceProvider serviceProvider)
    {
        builder.AddInterceptors(serviceProvider.GetRequiredService<AuditableEntitySaveChangesInterceptor>());
        return builder;
    }
}
