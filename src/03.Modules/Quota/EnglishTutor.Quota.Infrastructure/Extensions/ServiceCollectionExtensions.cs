using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EnglishTutor.Quota.Contracts;
using EnglishTutor.Quota.Application;
using EnglishTutor.Quota.Application.Abstractions.Persistence;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent.Repositories;
using EnglishTutor.Quota.Infrastructure.Persistence;
using EnglishTutor.Quota.Infrastructure.Persistence.Repositories;
using EnglishTutor.BuildingBlocks.Application;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using System;
using FluentValidation;

namespace EnglishTutor.Quota.Infrastructure.Extensions;

/// <summary>
/// Extension methods for adding Quota module services to the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Quota module services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The configuration to load connection strings from.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddQuotaModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default must be configured for Quota.");

        // Register QuotaDbContext with the AuditableEntitySaveChangesInterceptor resolved from service provider
        services.AddDbContext<QuotaDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(sp);
        });

        // Add MediatR handlers from the Quota Application assembly
        services.AddValidatorsFromAssembly(typeof(QuotaService).Assembly);
        services.AddLicensedMediatR(configuration["MediatR:LicenseKey"], typeof(QuotaService).Assembly);

        // Add the application service (the use case handler)
        services.AddScoped<IQuotaModule, QuotaService>();

        // Add the infrastructure repositories
        services.AddScoped<IUserQuotaRuleRepository, UserQuotaRuleRepository>();
        services.AddScoped<IUserQuotaStateRepository, UserQuotaStateRepository>();
        services.AddScoped<IQuotaReservationRepository, QuotaReservationRepository>();
        services.AddScoped<IUsageLogRepository, UsageLogRepository>();
        services.AddScoped<IRateLimitEventRepository, RateLimitEventRepository>();

        // Add the unit of work
        services.AddScoped<IQuotaUnitOfWork, QuotaUnitOfWork>();

        return services;
    }
}
