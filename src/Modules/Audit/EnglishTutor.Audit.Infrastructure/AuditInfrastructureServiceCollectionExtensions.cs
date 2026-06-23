using EnglishTutor.Audit.Domain.Aggregates.AuditLogs.Repositories;
using EnglishTutor.Audit.Application.Abstractions.Persistence;
using EnglishTutor.Audit.Application.Commands.RecordSecurityEvent;
using EnglishTutor.Audit.Contracts;
using EnglishTutor.Audit.Domain.Aggregates.SecurityEvents.Repositories;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.Audit.Infrastructure.Persistence.Repositories;
using EnglishTutor.Audit.Infrastructure.SecurityEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Audit.Infrastructure;

public static class AuditInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddAudit(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Audit")
            ?? configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Audit (or ConnectionStrings:Default) must be configured for Audit.");

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
            typeof(RecordSecurityEventCommand).Assembly));

        services.AddScoped<ISecurityEventRepository, SecurityEventRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();
        services.AddScoped<IAuditUnitOfWork, AuditUnitOfWork>();
        services.AddScoped<IAuditQueryService, AuditQueryService>();

        services.AddScoped<ISecurityEventRecorder, SecurityEventRecorder>();
        services.AddScoped<IAuditModule, AuditModule>();

        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        // Domain event dispatcher is registered at the API composition
        // root or at the module's Infrastructure extension, not via
        // reflection. This AddAudit extension does not register the
        // dispatcher; the composition root does.

        return services;
    }
}
