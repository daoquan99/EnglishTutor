using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Commands.SubmitAssessment;
using EnglishTutor.Modules.Assessments.Infrastructure.Persistence;
using EnglishTutor.Modules.Assessments.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.Assessments.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Assessments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAssessmentsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<AssessmentsDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "assessments"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IAssessmentDefinitionRepository, AssessmentDefinitionRepository>();
        services.AddScoped<IAssessmentAttemptRepository, AssessmentAttemptRepository>();
        services.AddScoped<IAssessmentRubricRepository, AssessmentRubricRepository>();
        services.AddScoped<IAssessmentsUnitOfWork>(provider => provider.GetRequiredService<AssessmentsDbContext>());
        services.AddScoped<AssessmentGradingOrchestrator>();
        services.AddScoped<AssessmentDataSeeder>();
        services.AddScoped<IModuleSeeder, AssessmentModuleSeeder>();

        return services;
    }
}
