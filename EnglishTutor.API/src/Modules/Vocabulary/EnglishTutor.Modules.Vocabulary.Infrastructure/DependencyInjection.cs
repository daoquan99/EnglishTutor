using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVocabularyModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<VocabularyDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "vocabulary"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IVocabularyItemRepository, VocabularyItemRepository>();
        services.AddScoped<IUserVocabularyMasteryRepository, UserVocabularyMasteryRepository>();
        services.AddScoped<IVocabularyReviewRepository, VocabularyReviewRepository>();
        services.AddScoped<IPronunciationAttemptRepository, PronunciationAttemptRepository>();
        services.AddScoped<IVocabularyStudySettingsRepository, VocabularyStudySettingsRepository>();
        services.AddScoped<IExampleFillBlankAttemptRepository, ExampleFillBlankAttemptRepository>();
        services.AddScoped<IVocabularyUnitOfWork>(provider => provider.GetRequiredService<VocabularyDbContext>());
        services.AddScoped<VocabularyDataSeeder>();
        services.AddScoped<IModuleSeeder, VocabularyModuleSeeder>();
        return services;
    }
}
