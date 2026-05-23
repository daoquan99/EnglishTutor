using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Seeding;
using EnglishTutor.BuildingBlocks.Infrastructure.Serialization;
using EnglishTutor.Modules.Exercises.Application.Abstractions;
using EnglishTutor.Modules.Exercises.Application.Commands.SubmitAnswer;
using EnglishTutor.Modules.Exercises.Infrastructure.Persistence;
using EnglishTutor.Modules.Exercises.Infrastructure.Persistence.Repositories;
using EnglishTutor.Modules.Exercises.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishTutor.Modules.Exercises.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddExercisesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<JsonSerializerService>();
        services.AddDbContext<ExercisesDbContext>((provider, options) =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "exercises"))
                .AddInterceptors(provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>()));

        services.AddScoped<IExerciseSetRepository, ExerciseSetRepository>();
        services.AddScoped<IExerciseAttemptRepository, ExerciseAttemptRepository>();
        services.AddScoped<IExercisesUnitOfWork>(provider => provider.GetRequiredService<ExercisesDbContext>());
        services.AddScoped<ExerciseGradingService>();
        services.AddScoped<ExerciseDataSeeder>();
        services.AddScoped<IModuleSeeder, ExerciseModuleSeeder>();

        return services;
    }
}
