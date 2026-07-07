using EnglishTutor.BuildingBlocks.Application;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Feedback.Contracts.Events;
using EnglishTutor.Practice.Contracts.Events;
using EnglishTutor.Progress.Domain.Aggregates.LearnerLanguageProgress.Repositories;
using EnglishTutor.Progress.Application.Progress.Queries.GetProgressSummary;
using EnglishTutor.Progress.Infrastructure.Consumers;
using EnglishTutor.Progress.Infrastructure.Persistence;
using EnglishTutor.Progress.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Progress.Infrastructure;

public static class ProgressServiceCollectionExtensions
{
    public static IServiceCollection AddProgressModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:Default must be configured for Progress.");
        services.AddDbContext<ProgressDbContext>((provider, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddAuditableEntityInterceptor(provider);
        });
        services.AddLicensedMediatR(
            configuration["MediatR:LicenseKey"],
            typeof(GetProgressSummaryQuery).Assembly);
        services.AddScoped<ILearnerLanguageProgressRepository, LearnerLanguageProgressRepository>();
        return services;
    }

    public static IServiceCollection AddProgressConsumers(this IServiceCollection services)
    {
        services.AddNativeRabbitMqConsumer<PracticeSessionEndedV2Consumer>(new(
            PracticeSessionEndedV2Consumer.ConsumerName,
            PracticeSessionEndedV2Consumer.QueueName,
            typeof(PracticeSessionEndedIntegrationEventV2),
            PrefetchCount: 16,
            Concurrency: 1));
        services.AddNativeRabbitMqConsumer<FeedbackReadyV2Consumer>(new(
            FeedbackReadyV2Consumer.ConsumerName,
            FeedbackReadyV2Consumer.QueueName,
            typeof(FeedbackReadyIntegrationEventV2),
            PrefetchCount: 16,
            Concurrency: 1));
        return services;
    }
}
