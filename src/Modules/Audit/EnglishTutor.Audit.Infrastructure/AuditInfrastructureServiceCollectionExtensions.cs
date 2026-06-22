using EnglishTutor.Audit.Application.Abstractions;
using EnglishTutor.Audit.Application.SecurityEvents;
using EnglishTutor.Audit.Contracts;
using EnglishTutor.Audit.Domain.SecurityEvents;
using EnglishTutor.Audit.Infrastructure.Audit.Contracts;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.Audit.Infrastructure.Persistence.Repositories;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
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
        services.AddScoped<IAuditUnitOfWork, AuditUnitOfWork>();

        services.AddScoped<ISecurityEventRecorder, SecurityEventRecorder>();

        services.AddDbContext<AuditDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddDomainEventDispatcher();

        return services;
    }
}
