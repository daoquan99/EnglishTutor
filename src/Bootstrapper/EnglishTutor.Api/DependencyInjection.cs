using System.Text;
using EnglishTutor.Api.OpenApi;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Behaviors;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.Infrastructure;
using EnglishTutor.Modules.AI.Infrastructure;
using EnglishTutor.Modules.Auth.Infrastructure;
using EnglishTutor.Modules.Mistakes.Infrastructure;
using EnglishTutor.Modules.Progress.Infrastructure;
using EnglishTutor.Modules.Speaking.Infrastructure;
using EnglishTutor.Modules.Users.Infrastructure;
using EnglishTutor.Modules.Vocabulary.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace EnglishTutor.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApiDocumentation();
        services.AddHealthChecks();
        services.AddHttpContextAccessor();

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            });
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"] ?? "super-secret-key-for-development-only-min-32-chars"))
                };
            });
        services.AddAuthorization();

        // BuildingBlocks registrations
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services
            .AddAuthModule(configuration)
            .AddUsersModule(configuration)
            .AddAiModule(configuration)
            .AddVocabularyModule(configuration)
            .AddSpeakingModule(configuration)
            .AddMistakesModule(configuration)
            .AddProgressModule(configuration);

        var applicationAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(assembly => assembly.GetName().Name?.EndsWith(".Application", StringComparison.Ordinal) == true)
            .Append(typeof(Result).Assembly)
            .Append(typeof(Program).Assembly)
            .Distinct()
            .ToArray();

        services.AddValidatorsFromAssemblies(applicationAssemblies);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblies(applicationAssemblies);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        });

        return services;
    }
}
