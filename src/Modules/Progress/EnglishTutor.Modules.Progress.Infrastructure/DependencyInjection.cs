using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.Modules.Mistakes.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.EventHandlers;
using EnglishTutor.Modules.Progress.Infrastructure.Persistence;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Progress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProgressModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProgressDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "progress")));

        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IProgressInboxStore, ProgressInboxStore>();
        services.AddScoped<IProgressUnitOfWork>(provider => provider.GetRequiredService<ProgressDbContext>());
        services.AddScoped<IIntegrationEventHandler<VocabularyReviewedIntegrationEvent>, VocabularyReviewedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<VocabularyPronunciationPracticedIntegrationEvent>, VocabularyPronunciationPracticedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ExampleSentencePronunciationPracticedIntegrationEvent>, ExampleSentencePronunciationPracticedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<SpeakingTurnCorrectedIntegrationEvent>, SpeakingTurnCorrectedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<SpeakingSessionCompletedIntegrationEvent>, SpeakingSessionCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<MistakeReviewedIntegrationEvent>, MistakeReviewedEventHandler>();

        return services;
    }
}
