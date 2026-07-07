using System;
using EnglishTutor.BuildingBlocks.Application;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Learning.Application.Abstractions.Persistence;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicPhrases.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.TopicVocabularies.Repositories;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Learning.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using EnglishTutor.Learning.Application.Abstractions.Messaging;
using EnglishTutor.Learning.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;
using EnglishTutor.Learning.Contracts.Events;
using EnglishTutor.Learning.Domain.Aggregates.Topics.Events;
using EnglishTutor.Learning.Domain.Aggregates.ModeDefinitions.Events;
using EnglishTutor.Learning.Domain.Aggregates.Scenarios.Events;
using EnglishTutor.Learning.Application.Messaging.DomainEventHandlers;
using EnglishTutor.Learning.Application.LanguagePairs.Services;
using EnglishTutor.Learning.Contracts;
using EnglishTutor.Learning.Domain.Aggregates.LanguageDefinitions.Repositories;
using EnglishTutor.Learning.Domain.Aggregates.LearnerLanguagePortfolios.Repositories;

namespace EnglishTutor.Learning.Infrastructure;

public static class LearningInfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddLearningInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register MediatR handlers + FluentValidation validators from Application assembly
        services.AddLicensedMediatR(configuration["MediatR:LicenseKey"], typeof(ILearningUnitOfWork).Assembly);
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
        services.AddScoped<ILanguageDefinitionRepository, LanguageDefinitionRepository>();
        services.AddScoped<ILearnerLanguagePortfolioRepository, LearnerLanguagePortfolioRepository>();
        services.AddScoped<ILearningLanguageModule, LearningLanguageModuleService>();
        services.AddScoped<LearningLanguageCatalogSeeder>();
        services.AddScoped<LearningContentCatalogSeeder>();
        services.AddScoped<LanguagePairPolicy>();

        // Register Unit of Work as scoped
        services.AddScoped<ILearningUnitOfWork, LearningUnitOfWork>();

        // Outbox Publisher
        services.AddScoped<NativeOutboxWriter<LearningDbContext>>();
        services.AddScoped<ILearningIntegrationEventPublisher, NativeLearningIntegrationEventPublisher>();
        services.AddScoped<IOutboxStore>(sp =>
            new EfOutboxStore<LearningDbContext>(
                sp.GetRequiredService<LearningDbContext>(),
                "Learning"));
        services.AddLearningMessageContracts();

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

    private static void AddLearningMessageContracts(this IServiceCollection services)
    {
        services.AddNativeMessageContract<TopicCreatedIntegrationEvent>("learning.topic.created.v1", MessageTopologyNames.IntegrationExchange, "learning.topic.created.v1");
        services.AddNativeMessageContract<TopicUpdatedIntegrationEvent>("learning.topic.updated.v1", MessageTopologyNames.IntegrationExchange, "learning.topic.updated.v1");
        services.AddNativeMessageContract<TopicDisabledIntegrationEvent>("learning.topic.disabled.v1", MessageTopologyNames.IntegrationExchange, "learning.topic.disabled.v1");
        services.AddNativeMessageContract<TopicModeEnabledIntegrationEvent>("learning.topic-mode.enabled.v1", MessageTopologyNames.IntegrationExchange, "learning.topic-mode.enabled.v1");
        services.AddNativeMessageContract<TopicModeDisabledIntegrationEvent>("learning.topic-mode.disabled.v1", MessageTopologyNames.IntegrationExchange, "learning.topic-mode.disabled.v1");
        services.AddNativeMessageContract<ModeDefinitionCreatedIntegrationEvent>("learning.mode.created.v1", MessageTopologyNames.IntegrationExchange, "learning.mode.created.v1");
        services.AddNativeMessageContract<ModeDefinitionUpdatedIntegrationEvent>("learning.mode.updated.v1", MessageTopologyNames.IntegrationExchange, "learning.mode.updated.v1");
        services.AddNativeMessageContract<ModeDefinitionDisabledIntegrationEvent>("learning.mode.disabled.v1", MessageTopologyNames.IntegrationExchange, "learning.mode.disabled.v1");
        services.AddNativeMessageContract<ScenarioCreatedIntegrationEvent>("learning.scenario.created.v1", MessageTopologyNames.IntegrationExchange, "learning.scenario.created.v1");
        services.AddNativeMessageContract<ScenarioUpdatedIntegrationEvent>("learning.scenario.updated.v1", MessageTopologyNames.IntegrationExchange, "learning.scenario.updated.v1");
        services.AddNativeMessageContract<ScenarioDisabledIntegrationEvent>("learning.scenario.disabled.v1", MessageTopologyNames.IntegrationExchange, "learning.scenario.disabled.v1");
    }
}
