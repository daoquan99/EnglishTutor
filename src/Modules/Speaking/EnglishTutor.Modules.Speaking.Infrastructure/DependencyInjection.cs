using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.Modules.Speaking.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Infrastructure.Persistence;
using EnglishTutor.Modules.Speaking.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Speaking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSpeakingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<SpeakingDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "speaking")));

        services.AddScoped<ISpeakingSessionRepository, SpeakingSessionRepository>();
        services.AddScoped<ISpeakingTurnRepository, SpeakingTurnRepository>();
        services.AddScoped<ISpeakingSessionSummaryRepository, SpeakingSessionSummaryRepository>();
        services.AddScoped<ISpeakingUnitOfWork>(provider => provider.GetRequiredService<SpeakingDbContext>());
        return services;
    }
}
