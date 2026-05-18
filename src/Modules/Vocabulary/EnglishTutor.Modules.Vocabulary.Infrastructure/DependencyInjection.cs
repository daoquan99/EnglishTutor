using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.Modules.Vocabulary.Application.Abstractions;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Vocabulary.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVocabularyModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<VocabularyDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "vocabulary")));

        services.AddScoped<IVocabularyItemRepository, VocabularyItemRepository>();
        services.AddScoped<IUserVocabularyMasteryRepository, UserVocabularyMasteryRepository>();
        services.AddScoped<IVocabularyReviewRepository, VocabularyReviewRepository>();
        services.AddScoped<IPronunciationAttemptRepository, PronunciationAttemptRepository>();
        services.AddScoped<IVocabularyUnitOfWork>(provider => provider.GetRequiredService<VocabularyDbContext>());
        return services;
    }
}
