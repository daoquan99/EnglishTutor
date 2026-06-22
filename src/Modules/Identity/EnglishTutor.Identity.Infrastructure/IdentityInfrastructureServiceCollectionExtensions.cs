using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Application.Services;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure.Persistence.Seed.Options;
using EnglishTutor.Identity.Infrastructure.Security;
using EnglishTutor.Identity.Infrastructure.Security.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EnglishTutor.Identity.Infrastructure;

public static class IdentityInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default must be configured for Identity.");

        // Technical options + startup validation (bound from configuration).
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<SeedDataOptions>()
            .Bind(configuration.GetSection(SeedDataOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register the audit interceptor once so DbContext can resolve it.
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        // Register the DbContext with the audit interceptor. We resolve the
        // interceptor from the same service collection so it shares the
        // scoped ICurrentUser/IDateTimeProvider with the request pipeline.
        services.AddDbContext<IdentityDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(sp);
        });

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        // Refresh-token abstractions (Application interfaces + Infrastructure impls).
        services.AddScoped<IRefreshTokenLifetimeProvider, RefreshTokenLifetimeProvider>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();

        services.AddScoped<IIdentityModuleDbContext, IdentityModuleDbContextAdapter>();
        services.AddScoped<IdentityDataSeeder>();

        return services;
    }
}
