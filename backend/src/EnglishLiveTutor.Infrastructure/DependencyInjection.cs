using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Infrastructure.Ai;
using EnglishLiveTutor.Infrastructure.Persistence;
using EnglishLiveTutor.Infrastructure.Realtime;
using EnglishLiveTutor.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishLiveTutor.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("SqlServer")
            ?? "Server=localhost;Database=EnglishLiveTutorDb;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IAppDbContextFactory, AppDbContextFactoryWrapper>();

        services.AddScoped<IConversationRepository, ConversationRepository>();

        services.AddHttpClient();

        services.AddSingleton<FastTextTranslatorFallback>();
        services.AddSingleton<GrammarCorrectionAnalyzerService>();

        services.AddScoped<IVocabularyAiGenerator, GeminiVocabularyAiGenerator>();
        services.AddScoped<IPronunciationAiAssessor, GeminiPronunciationAiAssessor>();

        services.AddSingleton<GeminiLiveConnectionManager>();

        return services;
    }
}
