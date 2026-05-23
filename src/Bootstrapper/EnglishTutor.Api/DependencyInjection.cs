using System.Text;
using EnglishTutor.Api.Hubs;
using EnglishTutor.Api.OpenApi;
using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Behaviors;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.BuildingBlocks.Infrastructure;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Storage;
using EnglishTutor.Modules.AdminReports.Infrastructure;
using EnglishTutor.Modules.AI.Infrastructure;
using EnglishTutor.Modules.Assessments.Infrastructure;
using EnglishTutor.Modules.Auth.Infrastructure;
using EnglishTutor.Modules.Auth.Presentation;
using EnglishTutor.Modules.Exercises.Infrastructure;
using EnglishTutor.Modules.LearningContent.Infrastructure;
using EnglishTutor.Modules.Mistakes.Infrastructure;
using EnglishTutor.Modules.Notifications.Infrastructure;
using EnglishTutor.Modules.Progress.Infrastructure;
using EnglishTutor.Modules.Speaking.Infrastructure;
using EnglishTutor.Modules.StudyPlans.Infrastructure;
using EnglishTutor.Modules.Users.Infrastructure;
using EnglishTutor.Modules.Vocabulary.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Threading.RateLimiting;

namespace EnglishTutor.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApiDocumentation();

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var redisConnectionString = configuration.GetConnectionString("Redis");
        var healthChecks = services.AddHealthChecks();
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            healthChecks.AddNpgSql(connectionString, name: "postgres", tags: ["db", "sql"]);
        }
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            healthChecks.AddRedis(redisConnectionString, name: "redis", tags: ["cache"]);
        }

        services.AddHttpContextAccessor();

        services.AddCors(options =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins")
                .Get<string[]>()?
                .Select(origin => origin.Trim())
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];
            var allowCredentials = configuration.GetValue("Cors:AllowCredentials", true);

            if (allowedOrigins.Length == 0)
            {
                throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");
            }

            options.AddPolicy("DefaultCors", policy =>
            {
                if (allowedOrigins.Any(origin => origin == "*"))
                {
                    if (allowCredentials)
                    {
                        throw new InvalidOperationException("Cors:AllowCredentials cannot be true when Cors:AllowedOrigins contains '*'.");
                    }

                    policy.AllowAnyOrigin();
                }
                else
                {
                    policy.WithOrigins(allowedOrigins);
                }

                policy.AllowAnyHeader()
                    .AllowAnyMethod();

                if (allowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });
        services.Configure<AuthCookieSettings>(configuration.GetSection("Auth:Cookies"));
        services.AddRateLimiter(options =>
        {
            // Per-IP rate limiter on authentication endpoints to mitigate credential-stuffing
            // and account-enumeration. Login/Register/Refresh share the bucket.
            options.AddPolicy("auth-credentials", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddFixedWindowLimiter("auth-refresh", policy =>
            {
                policy.PermitLimit = 20;
                policy.Window = TimeSpan.FromMinutes(1);
                policy.QueueLimit = 0;
                policy.AutoReplenishment = true;
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        var jwtSecret = configuration["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret) || Encoding.UTF8.GetByteCount(jwtSecret) < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be configured and contain at least 32 UTF-8 bytes.");
        }

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
                    // Default ClockSkew is 5 minutes — that silently extends every access token
                    // past its exp claim. Honour the exp claim exactly.
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();

        services.AddSignalR();

        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.AddSingleton<IConnectionMultiplexer>(
                ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddHostedService<RealtimeNotificationRelay>();
        }

        // BuildingBlocks registrations
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddFileStorage(configuration);

        services
            .AddAuthModule(configuration)
            .AddUsersModule(configuration)
            .AddAiModule(configuration)
            .AddVocabularyModule(configuration)
            .AddSpeakingModule(configuration)
            .AddMistakesModule(configuration)
            .AddProgressModule(configuration)
            .AddStudyPlansModule(configuration)
            .AddNotificationsModule(configuration)
            .AddLearningContentModule(configuration)
            .AddExercisesModule(configuration)
            .AddAssessmentsModule(configuration)
            .AddAdminReportsModule(configuration);

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
