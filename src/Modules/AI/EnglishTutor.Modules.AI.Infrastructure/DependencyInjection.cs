using EnglishTutor.Modules.AI.Application.Abstractions;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.AI.Infrastructure.Clients;
using EnglishTutor.Modules.AI.Infrastructure.ContractImplementations;
using EnglishTutor.Modules.AI.Infrastructure.Persistence;
using EnglishTutor.Modules.AI.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.AI.Infrastructure.Prompts;
using EnglishTutor.Modules.AI.Infrastructure.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.AI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAiModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AiDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ai")));

        services.AddScoped<GeminiClient>();
        services.AddScoped<GemmaClient>();
        services.AddScoped<IAiClient, AiClientFactory>();
        services.AddScoped<IModelRouter, ModelRouter>();
        services.AddScoped<IPromptBuilder, PromptBuilder>();
        services.AddScoped<IAiRequestLogRepository, AiRequestLogRepository>();
        services.AddScoped<IPromptTemplateRepository, PromptTemplateRepository>();
        services.AddScoped<IModelRoutingRuleRepository, ModelRoutingRuleRepository>();
        services.AddScoped<IEnglishCorrectionService, EnglishCorrectionService>();

        return services;
    }
}
