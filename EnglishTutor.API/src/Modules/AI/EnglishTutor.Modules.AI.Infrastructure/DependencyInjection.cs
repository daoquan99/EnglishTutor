using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;
using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.AI.Infrastructure.Clients;
using EnglishTutor.Modules.AI.Infrastructure.ContractImplementations;
using EnglishTutor.Modules.AI.Infrastructure.Options;
using EnglishTutor.Modules.AI.Infrastructure.Persistence;
using EnglishTutor.Modules.AI.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.AI.Infrastructure.Prompts;
using EnglishTutor.Modules.AI.Infrastructure.Routing;
using EnglishTutor.Modules.AI.Infrastructure.Secrets;
using EnglishTutor.Modules.AI.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.AI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AiDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ai"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddSingleton(AiProviderOptions.FromConfiguration(configuration));
        services.AddSingleton<ISecretProvider, ConfigurationSecretProvider>();
        services.AddHttpClient<GeminiClient>(client => client.Timeout = TimeSpan.FromSeconds(60));
        services.AddScoped<GemmaClient>();
        services.AddHttpClient<OpenAiCompatibleClient>(client => client.Timeout = TimeSpan.FromSeconds(60));
        services.AddScoped<DeepSeekClient>();
        services.AddSingleton<GeminiLiveClient>();
        services.AddScoped<IAiClient, AiClientFactory>();
        services.AddScoped<IModelRouter, ModelRouter>();
        services.AddScoped<IAiRuntimeRouter, AiRuntimeRouter>();
        services.AddScoped<IPromptBuilder, PromptBuilder>();
        services.AddScoped<IAiRequestLogRepository, AiRequestLogRepository>();
        services.AddScoped<IPromptTemplateRepository, PromptTemplateRepository>();
        services.AddScoped<IModelRoutingRuleRepository, ModelRoutingRuleRepository>();
        services.AddScoped<IAiProviderRepository, AiProviderRepository>();
        services.AddScoped<IAiRuntimeRouteRepository, AiRuntimeRouteRepository>();
        services.AddScoped<IEnglishCorrectionService, EnglishCorrectionService>();
        services.AddScoped<IAssessmentGradingService, AssessmentGradingService>();
        services.AddScoped<IAudioGenerationService, AudioGenerationService>();
        services.AddScoped<IPronunciationScoringService, PronunciationScoringService>();
        services.AddSingleton<IRealtimeVoiceService, RealtimeVoiceService>();
        services.AddScoped<AiDataSeeder>();
        services.AddScoped<IModuleSeeder, AiModuleSeeder>();

        return services;
    }
}
