using System;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Practice.Application;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using EnglishTutor.Practice.Application.Sessions.Abstractions;
using EnglishTutor.Practice.Infrastructure.Sessions;
using EnglishTutor.Practice.Contracts;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeScenarioReadModel.Repositories;
using EnglishTutor.Practice.Infrastructure.Persistence;
using EnglishTutor.Practice.Infrastructure.Persistence.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using EnglishTutor.Practice.Application.Abstractions.Messaging;
using EnglishTutor.Practice.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Events;
using EnglishTutor.Practice.Application.Messaging.DomainEventHandlers;

namespace EnglishTutor.Practice.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPracticeModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default must be configured for Practice.");

        services.AddDbContext<PracticeDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(sp);
        });

        // Register MediatR & FluentValidation for Practice application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IPracticeUnitOfWork).Assembly));
        services.AddValidatorsFromAssembly(typeof(IPracticeUnitOfWork).Assembly);

        // Register internal services
        services.AddScoped<IPracticeSessionResourceFinalizer, PracticeSessionResourceFinalizer>();

        services.AddScoped<IPracticeModule, PracticeService>();

        services.AddScoped<IPracticeSessionRepository, PracticeSessionRepository>();
        services.AddScoped<IPracticeScenarioReadModelRepository, PracticeScenarioReadModelRepository>();
        services.AddScoped<IPracticeUnitOfWork, PracticeUnitOfWork>();
        services.AddScoped<IPracticeIntegrationEventPublisher, MassTransitPracticeIntegrationEventPublisher>();

        // Domain Event Handlers & Dispatcher
        services.AddDomainEventDispatcher();
        services.AddDomainEventHandler<PracticeSessionStartedDomainEvent, PracticeSessionStartedDomainEventHandler>();
        services.AddDomainEventHandler<PracticeSessionEndedDomainEvent, PracticeSessionEndedDomainEventHandler>();

        return services;
    }
}
