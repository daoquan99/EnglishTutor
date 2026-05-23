using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Modules.Exercises.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Mistakes.Contracts.IntegrationEvents;
using EnglishTutor.Modules.LearningContent.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Progress.Application.Abstractions;
using EnglishTutor.Modules.Progress.Application.EventHandlers;
using EnglishTutor.Modules.Progress.Infrastructure.Persistence;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.StudyPlans.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Vocabulary.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Progress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProgressModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProgressDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "progress"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IProgressRepository, ProgressRepository>();
        services.AddScoped<IProgressInboxStore, ProgressInboxStore>();
        services.AddScoped<IProgressUnitOfWork>(provider => provider.GetRequiredService<ProgressDbContext>());
        services.AddScoped<IIntegrationEventHandler<VocabularyReviewedIntegrationEvent>, VocabularyReviewedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<VocabularyPronunciationPracticedIntegrationEvent>, VocabularyPronunciationPracticedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ExampleSentencePronunciationPracticedIntegrationEvent>, ExampleSentencePronunciationPracticedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<SpeakingTurnCorrectedIntegrationEvent>, SpeakingTurnCorrectedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<SpeakingSessionCompletedIntegrationEvent>, SpeakingSessionCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<MistakeReviewedIntegrationEvent>, MistakeReviewedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<LessonCompletedIntegrationEvent>, LessonCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ConversationScenarioCompletedIntegrationEvent>, ConversationScenarioCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<DailyStudyTargetCompletedIntegrationEvent>, DailyStudyTargetCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<ExerciseCompletedIntegrationEvent>, ExerciseCompletedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<UserLevelChangedIntegrationEvent>, UserLevelChangedEventHandler>();

        return services;
    }
}
