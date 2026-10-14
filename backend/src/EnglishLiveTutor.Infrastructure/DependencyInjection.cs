using EnglishLiveTutor.Application.Common.Interfaces;
using EnglishLiveTutor.Application.Features.Identity;
using EnglishLiveTutor.Domain.Identity;
using EnglishLiveTutor.Infrastructure.Identity;
using EnglishLiveTutor.Infrastructure.Ai;
using EnglishLiveTutor.Infrastructure.Persistence;
using EnglishLiveTutor.Infrastructure.Persistence.Interceptors;
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
        string connectionString = configuration.GetConnectionString("DefaultConnection")!;
        // Never print database connection strings (may include credentials).
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<AuditingSaveChangesInterceptor>();
        services.AddDbContextFactory<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);
            options.AddInterceptors(sp.GetRequiredService<AuditingSaveChangesInterceptor>());
        });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddSingleton<IAppDbContextFactory, AppDbContextFactoryWrapper>();

        services.AddSingleton<IConversationRepository, ConversationRepository>();

        services.AddHttpClient();

        services.AddSingleton<FastTextTranslatorFallback>();
        services.AddSingleton<GrammarCorrectionAnalyzerService>();

        services.AddScoped<IVocabularyAiGenerator, GeminiVocabularyAiGenerator>();
        services.AddScoped<IPronunciationAiAssessor, GeminiPronunciationAiAssessor>();

        services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<UserEntity>,
            Microsoft.AspNetCore.Identity.PasswordHasher<UserEntity>>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddSingleton<ICefrPromptBuilder, CefrPromptBuilder>();

        services.AddSingleton<GeminiLiveConnectionManager>();

        return services;
    }
}
