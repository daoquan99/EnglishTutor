using System;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Learning.Infrastructure.Persistence.Repositories;
using MediatR;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using EnglishTutor.Learning.Application.Abstractions.Messaging;
using EnglishTutor.Learning.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Events;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Events;
using EnglishTutor.Learning.Application.Messaging.DomainEventHandlers;

namespace EnglishTutor.Learning.Infrastructure;

public static class LearningInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddLearningInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register MediatR handlers + FluentValidation validators from Application assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ILearningUnitOfWork).Assembly));
        services.AddValidatorsFromAssembly(typeof(ILearningUnitOfWork).Assembly);
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default must be configured for Learning.");

        // Register DbContext with the AuditableEntitySaveChangesInterceptor resolved from service provider
        services.AddDbContext<LearningDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(sp);
        });

        // Register repositories as scoped
        services.AddScoped<ITopicRepository, TopicRepository>();
        services.AddScoped<IModeDefinitionRepository, ModeDefinitionRepository>();
        services.AddScoped<IScenarioRepository, ScenarioRepository>();
        services.AddScoped<ITopicVocabularyRepository, TopicVocabularyRepository>();
        services.AddScoped<ITopicPhraseRepository, TopicPhraseRepository>();

        // Register Unit of Work as scoped
        services.AddScoped<ILearningUnitOfWork, LearningUnitOfWork>();

        // Outbox Publisher
        services.AddScoped<ILearningIntegrationEventPublisher, MassTransitLearningIntegrationEventPublisher>();

        // Domain Event Handlers
        services.AddDomainEventDispatcher();
        services.AddDomainEventHandler<TopicCreatedDomainEvent, TopicCreatedDomainEventHandler>();
        services.AddDomainEventHandler<TopicUpdatedDomainEvent, TopicUpdatedDomainEventHandler>();
        services.AddDomainEventHandler<TopicDisabledDomainEvent, TopicDisabledDomainEventHandler>();
        services.AddDomainEventHandler<TopicModeEnabledDomainEvent, TopicModeEnabledDomainEventHandler>();
        services.AddDomainEventHandler<TopicModeDisabledDomainEvent, TopicModeDisabledDomainEventHandler>();

        services.AddDomainEventHandler<ModeDefinitionCreatedDomainEvent, ModeDefinitionCreatedDomainEventHandler>();
        services.AddDomainEventHandler<ModeDefinitionUpdatedDomainEvent, ModeDefinitionUpdatedDomainEventHandler>();
        services.AddDomainEventHandler<ModeDefinitionDisabledDomainEvent, ModeDefinitionDisabledDomainEventHandler>();

        services.AddDomainEventHandler<ScenarioCreatedDomainEvent, ScenarioCreatedDomainEventHandler>();
        services.AddDomainEventHandler<ScenarioUpdatedDomainEvent, ScenarioUpdatedDomainEventHandler>();
        services.AddDomainEventHandler<ScenarioDisabledDomainEvent, ScenarioDisabledDomainEventHandler>();

        return services;
    }
}
