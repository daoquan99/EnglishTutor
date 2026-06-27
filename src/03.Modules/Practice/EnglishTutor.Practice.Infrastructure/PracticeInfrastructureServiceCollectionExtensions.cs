using System;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;
using EnglishTutor.Practice.Infrastructure.Persistence;
using EnglishTutor.Practice.Infrastructure.Persistence.Repositories;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Practice.Infrastructure;

public static class PracticeInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPracticeInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Register MediatR handlers + FluentValidation validators from Application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IPracticeUnitOfWork).Assembly));
        services.AddValidatorsFromAssembly(typeof(IPracticeUnitOfWork).Assembly);

        // 2. Resolve Connection String
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("ConnectionStrings:Default must be configured.");

        // 3. Register DbContext with the AuditableEntitySaveChangesInterceptor resolved from service provider
        services.AddDbContext<PracticeDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(sp);
        });

        // 4. Register Repositories
        services.AddScoped<IPracticeSessionRepository, PracticeSessionRepository>();
        services.AddScoped<IPracticeScenarioReadModelRepository, PracticeScenarioReadModelRepository>();

        // 5. Register Unit of Work
        services.AddScoped<IPracticeUnitOfWork, PracticeUnitOfWork>();

        return services;
    }
}
