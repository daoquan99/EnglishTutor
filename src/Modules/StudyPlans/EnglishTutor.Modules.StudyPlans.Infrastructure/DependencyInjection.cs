using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.Modules.StudyPlans.Application.Abstractions;
using EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence;
using EnglishTutor.Modules.StudyPlans.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.StudyPlans.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStudyPlansModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<StudyPlansDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "studyplans"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IStudyPlanRepository, StudyPlanRepository>();
        services.AddScoped<IPlannedStudySessionRepository, PlannedStudySessionRepository>();
        services.AddScoped<IStudyPlansUnitOfWork>(provider => provider.GetRequiredService<StudyPlansDbContext>());

        return services;
    }
}
