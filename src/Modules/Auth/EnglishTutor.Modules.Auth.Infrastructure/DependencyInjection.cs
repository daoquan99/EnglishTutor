using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Infrastructure.Authentication;
using EnglishTutor.Modules.Auth.Infrastructure.Caching;
using EnglishTutor.Modules.Auth.Infrastructure.Persistence;
using EnglishTutor.Modules.Auth.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.Auth.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<AuthDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "auth"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            options.InstanceName = "EnglishTutor:";
        });

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddScoped<IAuthSessionRepository, AuthSessionRepository>();
        services.AddScoped<IAuthSecurityEventRepository, AuthSecurityEventRepository>();
        services.AddScoped<IAuthPermissionRepository, AuthPermissionRepository>();
        services.AddScoped<IAuthRolePermissionRepository, AuthRolePermissionRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthDomainEventToOutboxMapper, AuthDomainEventToOutboxMapper>();
        services.AddScoped<IAuthUnitOfWork>(provider => provider.GetRequiredService<AuthDbContext>());
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
        services.AddScoped<IRefreshTokenHasher, RefreshTokenHasher>();
        services.AddScoped<IRefreshTokenCache, RedisRefreshTokenCache>();
        services.AddScoped<AuthDataSeeder>();
        services.AddScoped<IModuleSeeder, AuthModuleSeeder>();

        return services;
    }
}
