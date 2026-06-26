using System.Text;
using EnglishTutor.Audit.Contracts;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Services;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Infrastructure.Audit;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure.Persistence.Repositories;
using EnglishTutor.Identity.Infrastructure.Persistence.Seed.Options;
using EnglishTutor.Identity.Infrastructure.Security;
using EnglishTutor.Identity.Infrastructure.Security.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

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

        // JWT bearer authentication. Required by /api/me and /api/auth/logout,
        // both of which carry .RequireAuthorization() metadata on their routes.
        // Signing key + issuer + audience are read from JwtOptions above.
        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwt = configuration
                    .GetSection(JwtOptions.SectionName)
                    .Get<JwtOptions>() ?? new JwtOptions();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.SigningKey)),
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        // Authorization services — required by app.UseAuthorization() in Program.cs.
        services.AddAuthorization();

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

        // Aggregate repositories (Application interfaces + Infrastructure impls).
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSessionRepository, UserSessionRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        // Unit of Work (Application interface + Infrastructure impl).
        services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();

        services.AddScoped<IIdentityModuleDbContext, IdentityModuleDbContextAdapter>();
        services.AddScoped<IdentityDataSeeder>();

        // HTTP context accessor + the per-request context accessor used by
        // RefreshTokenReuseAuditHandler to read IP / User-Agent for the
        // current request.
        services.AddHttpContextAccessor();
        services.AddScoped<IRefreshTokenReuseContextAccessor, HttpContextRefreshTokenReuseContextAccessor>();
        services.AddScoped<IIdentitySecurityEventPublisher, OutboxIdentitySecurityEventPublisher>();
        services.AddScoped<IIdentitySecurityEventService, IdentitySecurityEventService>();

        // Domain event dispatcher. The InMemory implementation lives in
        // BuildingBlocks.Infrastructure. Identity.Infrastructure adds the
        // handler that bridges RefreshTokenReuseDetectedDomainEvent to the
        // Audit.Contracts.ISecurityEventRecorder implementation.
        services.AddDomainEventDispatcher();
        services.AddDomainEventHandler<RefreshTokenReuseDetectedDomainEvent, RefreshTokenReuseAuditHandler>();
        services.AddDomainEventHandler<UserSessionCreatedDomainEvent, UserSessionCreatedAuditHandler>();
        services.AddDomainEventHandler<RefreshTokenRotatedDomainEvent, RefreshTokenRotatedAuditHandler>();
        services.AddDomainEventHandler<UserSessionRevokedDomainEvent, UserSessionRevokedAuditHandler>();

        return services;
    }
}
