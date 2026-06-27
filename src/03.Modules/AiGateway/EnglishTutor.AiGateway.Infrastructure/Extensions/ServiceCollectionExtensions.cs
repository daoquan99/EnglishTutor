using System;
using EnglishTutor.AiGateway.Application;
using EnglishTutor.AiGateway.Application.Abstractions;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Providers;
using EnglishTutor.AiGateway.Application.Abstractions.Security;
using EnglishTutor.AiGateway.Contracts;
using FluentValidation;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease.Repositories;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRoutingRule.Repositories;
using EnglishTutor.AiGateway.Infrastructure.Audit;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using EnglishTutor.AiGateway.Infrastructure.Persistence.Repositories;
using EnglishTutor.AiGateway.Infrastructure.Providers;
using EnglishTutor.AiGateway.Infrastructure.Security;
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

        services
            .AddOptions<AiGatewayOptions>()
            .Bind(configuration.GetSection(AiGatewayOptions.SectionName));

        services.AddDbContext<AiGatewayDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(sp);
        });

        services.AddScoped<IAiGatewayModule, AiGatewayService>();
        services.AddValidatorsFromAssembly(typeof(AiGatewayService).Assembly);
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AiGatewayService).Assembly));

        services.AddScoped<IAiProviderRepository, AiProviderRepository>();
        services.AddScoped<IAiModelRepository, AiModelRepository>();
        services.AddScoped<IAiProviderKeyRepository, AiProviderKeyRepository>();
        services.AddScoped<IAiRoutingRuleRepository, AiRoutingRuleRepository>();
        services.AddScoped<IAiRouteLeaseRepository, AiRouteLeaseRepository>();

        services.AddScoped<IAiGatewayUnitOfWork, AiGatewayUnitOfWork>();

        // Provider key protection (AES-GCM at rest) + audit forwarding.
        services.AddSingleton<IAiKeyProtector, AesGcmAiKeyProtector>();
        services.AddScoped<IAiGatewayAuditPort, AuditModuleAiGatewayAuditPort>();

        // Provider adapters + execution gateway. The mock adapter is the safe
        // in-process baseline; real provider adapters register the same interface.
        services.AddSingleton<IAiProviderAdapter, MockProviderAdapter>();
        services.AddSingleton<IAiProviderAdapterRegistry, AiProviderAdapterRegistry>();
        services.AddScoped<IAiProviderExecutionGateway, AiProviderExecutionGateway>();

        return services;
    }
}
