using EnglishTutor.BuildingBlocks.EventBus;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.EventHandlers;
using EnglishTutor.Modules.LearningContent.Contracts.Readers;
using EnglishTutor.Modules.LearningContent.Infrastructure.ContractReaders;
using EnglishTutor.Modules.LearningContent.Infrastructure.Persistence;
using EnglishTutor.Modules.LearningContent.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.LearningContent.Infrastructure.Seed;
using EnglishTutor.Modules.Speaking.Contracts.IntegrationEvents;
using EnglishTutor.Modules.Users.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.LearningContent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddLearningContentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<LearningContentDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "learningcontent"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<IConversationScenarioRepository, ConversationScenarioRepository>();
        services.AddScoped<ILearningPathCardRepository, LearningPathCardRepository>();
        services.AddScoped<ILearningContentInboxStore, LearningContentInboxStore>();
        services.AddScoped<ILearningContentUnitOfWork>(provider => provider.GetRequiredService<LearningContentDbContext>());
        services.AddScoped<IConversationScenarioReader, ConversationScenarioReader>();
        services.AddScoped<LearningContentDataSeeder>();
        services.AddScoped<IModuleSeeder, LearningContentModuleSeeder>();

        services.AddScoped<IIntegrationEventHandler<UserLevelChangedIntegrationEvent>, UserLevelChangedEventHandler>();
        services.AddScoped<IIntegrationEventHandler<SpeakingSessionCompletedIntegrationEvent>, SpeakingSessionCompletedEventHandler>();

        return services;
    }
}
