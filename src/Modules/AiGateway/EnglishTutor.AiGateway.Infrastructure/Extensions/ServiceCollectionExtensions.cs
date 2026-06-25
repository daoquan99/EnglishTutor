using System;
using EnglishTutor.AiGateway.Application;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule.Repositories;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.AiGateway.Infrastructure.Extensions;

/// <summary>
/// Extension methods for adding AI Gateway module services to the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the AI Gateway module services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddAiGatewayModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default must be configured for AI Gateway.");

        // Register AiGatewayDbContext with the AuditableEntitySaveChangesInterceptor resolved from service provider
        services.AddDbContext<AiGatewayDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(sp);
        });

        // Add the application service contract
        services.AddScoped<IAiGatewayModule, AiGatewayService>();

        // Add the infrastructure repositories
        services.AddScoped<IAiProviderRepository, AiProviderRepository>();
        services.AddScoped<IAiModelRepository, AiModelRepository>();
        services.AddScoped<IAiProviderKeyRepository, AiProviderKeyRepository>();
        services.AddScoped<IAiRoutingRuleRepository, AiRoutingRuleRepository>();
        services.AddScoped<IAiRouteLeaseRepository, AiRouteLeaseRepository>();

        // Add the unit of work
        services.AddScoped<IAiGatewayUnitOfWork, AiGatewayUnitOfWork>();

        return services;
    }
}
