using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Mistakes.Application.Abstractions;
using EnglishTutor.Modules.Mistakes.Application.EventHandlers;
using EnglishTutor.Modules.Mistakes.Infrastructure.Persistence;
using EnglishTutor.Modules.Mistakes.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Mistakes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMistakesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<MistakesDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "mistakes"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IMistakeRepository, MistakeRepository>();
        services.AddScoped<IMistakesUnitOfWork>(provider => provider.GetRequiredService<MistakesDbContext>());
        services.AddScoped<IMistakesInboxStore, MistakesInboxStore>();
        services.AddScoped<IIntegrationEventHandler<SpeakingTurnCorrectedIntegrationEvent>, SpeakingTurnCorrectedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<VocabularyPronunciationPracticedIntegrationEvent>, VocabularyPronunciationPracticedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ExampleSentencePronunciationPracticedIntegrationEvent>, ExampleSentencePronunciationPracticedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ExerciseCompletedIntegrationEvent>, ExerciseCompletedEventHandler>();

        return services;
    }
}
