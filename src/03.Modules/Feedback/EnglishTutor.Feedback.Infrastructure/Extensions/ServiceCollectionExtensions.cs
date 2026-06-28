using System;
using EnglishTutor.BuildingBlocks.Application;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Feedback.Application.Abstractions.Persistence;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Repositories;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using EnglishTutor.Feedback.Infrastructure.Persistence.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using EnglishTutor.Feedback.Application.Abstractions.Messaging;
using EnglishTutor.Feedback.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Infrastructure.Outbox;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Feedback.Domain.Aggregates.SessionFeedback.Events;
using EnglishTutor.Feedback.Application.Messaging.DomainEventHandlers;

namespace EnglishTutor.Feedback.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeedbackModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default must be configured for Feedback.");

        services.AddDbContext<FeedbackDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(sp);
        });

        // Register MediatR & FluentValidation for Feedback application assembly
        services.AddLicensedMediatR(configuration["MediatR:LicenseKey"], typeof(IFeedbackUnitOfWork).Assembly);
        services.AddValidatorsFromAssembly(typeof(IFeedbackUnitOfWork).Assembly);

        // Register Repositories & Unit of Work
        services.AddScoped<ISessionFeedbackRepository, SessionFeedbackRepository>();
        services.AddScoped<IFeedbackUnitOfWork, FeedbackUnitOfWork>();

        // Register Facade Service
        services.AddScoped<EnglishTutor.Feedback.Contracts.IFeedbackModule, EnglishTutor.Feedback.Application.FeedbackService>();

        // Outbox Publisher
        services.AddScoped<NativeOutboxWriter<FeedbackDbContext>>();
        services.AddScoped<IFeedbackIntegrationEventPublisher, NativeFeedbackIntegrationEventPublisher>();
        services.AddScoped<IOutboxStore>(sp =>
            new EfOutboxStore<FeedbackDbContext>(
                sp.GetRequiredService<FeedbackDbContext>(),
                "Feedback"));
        services.AddNativeMessageContract<FeedbackReadyIntegrationEventV1>(
            "feedback.session.ready.v1",
            MessageTopologyNames.IntegrationExchange,
            "feedback.session.ready.v1");
        services.AddNativeMessageContract<GenerateSessionFeedbackRequestedV1>(
            "feedback.generate-session.v1",
            MessageTopologyNames.CommandExchange,
            "feedback.generate-session.v1");
        services.AddNativeMessageContract<ExtractVocabularyRequestedV1>(
            "feedback.extract-vocabulary.v1",
            MessageTopologyNames.CommandExchange,
            "feedback.extract-vocabulary.v1");

        // Domain Event Handlers
        services.AddDomainEventDispatcher();
        services.AddDomainEventHandler<FeedbackCompletedDomainEvent, FeedbackCompletedDomainEventHandler>();

        return services;
    }
}
