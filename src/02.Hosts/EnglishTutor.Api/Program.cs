
using EnglishTutor.AiGateway.Infrastructure.Extensions;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using EnglishTutor.AiGateway.Presentation;
using EnglishTutor.Practice.Infrastructure.Extensions;
using EnglishTutor.Practice.Presentation;
using EnglishTutor.Feedback.Presentation;
using EnglishTutor.Realtime.Infrastructure.Extensions;
using EnglishTutor.Realtime.Infrastructure.Messaging;
using EnglishTutor.Realtime.Presentation;
using EnglishTutor.Audit.Infrastructure;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.Audit.Presentation;
using EnglishTutor.BuildingBlocks.Infrastructure.Correlation;
using EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using EnglishTutor.BuildingBlocks.Presentation.Responses;
using EnglishTutor.Api.Configuration;
using EnglishTutor.Identity.Application;
using EnglishTutor.Identity.Infrastructure;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Presentation;
using EnglishTutor.Learning.Infrastructure;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Learning.Presentation;
using EnglishTutor.Quota.Infrastructure.Extensions;
using EnglishTutor.Quota.Infrastructure.Persistence;
using EnglishTutor.Feedback.Infrastructure.Extensions;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using EnglishTutor.Practice.Infrastructure.Persistence;
using EnglishTutor.Progress.Infrastructure;
using EnglishTutor.Progress.Infrastructure.Persistence;
using EnglishTutor.Progress.Presentation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;

RepositoryEnvironment.LoadIntoProcess();

var builder = WebApplication.CreateBuilder(args);
const string DefaultCorsPolicy = "DefaultCors";

builder.AddServiceDefaults();

builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// Strongly-typed options + startup validation
builder.Services.AddBaseOptions(builder.Configuration);
builder.Services.AddNativeMessagingRegistry();
builder.Services.AddApiPresentation();
builder.Services.AddOpenApi();
var corsSection = builder.Configuration.GetSection(CorsOptions.SectionName);
var corsOptions = corsSection.Get<CorsOptions>() ?? new CorsOptions();

builder.Services
    .AddOptions<CorsOptions>()
    .Bind(corsSection)
    .Validate(
        options => builder.Environment.IsDevelopment()
            ? options.AllowAnyOriginInDevelopment || options.AllowedOrigins.Length > 0
            : options.AllowedOrigins.Length > 0,
        "Cors:AllowedOrigins must contain at least one origin outside unrestricted Development mode.")
    .ValidateOnStart();

builder.Services.AddCors(options =>
{
    options.AddPolicy(DefaultCorsPolicy, policy =>
    {
        if (builder.Environment.IsDevelopment() && corsOptions.AllowAnyOriginInDevelopment)
        {
            policy
                .SetIsOriginAllowed(_ => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy
                .WithOrigins(corsOptions.AllowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

// Health checks: liveness + readiness (Postgres / RabbitMQ / Redis)
builder.Services.AddBaseHealthChecks();
builder.Services.AddInfrastructureHealthChecks();

// Identity module (Application + Infrastructure + Presentation)
builder.Services.AddIdentityApplication(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddIdentityPresentation(builder.Configuration);
builder.Services.AddAudit(builder.Configuration);
builder.Services.AddAuditPresentation(builder.Configuration);
builder.Services.AddLearningInfrastructure(builder.Configuration);
builder.Services.AddLearningPresentation(builder.Configuration);
builder.Services.AddQuotaModule(builder.Configuration);
builder.Services.AddAiGatewayModule(builder.Configuration);
builder.Services.AddPracticeModule(builder.Configuration);
builder.Services.AddFeedbackModule(builder.Configuration);
builder.Services.AddFeedbackPresentation(builder.Configuration);
builder.Services.AddRealtimeInfrastructure(builder.Configuration);
builder.Services.AddRealtimePresentation(builder.Configuration);
builder.Services.AddProgressModule(builder.Configuration);
builder.Services.AddProgressPresentation();
builder.Services.AddRealtimeConsumers();
builder.Services.AddNativeRabbitMqConsumerRuntime(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var databaseStartup = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseStartupOptions>>().Value;

    try
    {
        await ApplyStartupMigrationsAsync(scope.ServiceProvider, databaseStartup, logger);

        if (databaseStartup.ApplySeedDataOnStartup)
        {
            await scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>().SeedAsync();
            await scope.ServiceProvider.GetRequiredService<GoogleAiCatalogSeeder>().SeedAsync();
            await scope.ServiceProvider.GetRequiredService<LearningLanguageCatalogSeeder>().SeedAsync();
            await scope.ServiceProvider.GetRequiredService<LearningContentCatalogSeeder>().SeedAsync();
        }
    }
    catch (Npgsql.NpgsqlException ex)
    {
        logger.LogWarning(ex, "Startup database migration/seeding skipped because PostgreSQL is unreachable.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup database migration/seeding failed.");
    }
}

// Middleware
app.UseExceptionHandler();
app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors(DefaultCorsPolicy);

// Authentication + Authorization — required because /api/me and
// /api/auth/logout carry .RequireAuthorization() metadata.
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// Health Checks
app.MapBaseHealthChecks();
app.MapOpenApi().AllowAnonymous();
app.MapScalarApiReference("/docs").AllowAnonymous();

// Identity endpoints
app.MapIdentityEndpoints();
app.MapAuditEndpoints();
app.MapLearningEndpoints();
app.MapAiGatewayEndpoints();
app.MapPracticeModuleEndpoints();
app.MapFeedbackModuleEndpoints();
app.MapRealtimeHubs();
app.MapProgressEndpoints();

// Minimal API root
app.MapGet("/", () => ApiResults.Ok(new
{
    service = "EnglishTutor.Api",
    version = "1.0.0",
    status = "Running",
    timestamp = DateTime.UtcNow
}))
.WithName("GetRoot")
.ExcludeFromDescription();

app.MapFallback("/api/{**path}", () =>
    ApiResults.Problem(
        statusCode: StatusCodes.Status404NotFound,
        code: ApiErrorCodes.NotFound,
        title: "Not found",
        message: "The requested API resource was not found."));

app.Run();

static async Task ApplyStartupMigrationsAsync(
    IServiceProvider serviceProvider,
    DatabaseStartupOptions options,
    ILogger<Program> logger)
{
    (string Name, Func<IServiceProvider, DbContext> Resolve)[] modules =
    [
        (DatabaseStartupModuleNames.Identity, sp => sp.GetRequiredService<IdentityDbContext>()),
        (DatabaseStartupModuleNames.Audit, sp => sp.GetRequiredService<AuditDbContext>()),
        (DatabaseStartupModuleNames.Learning, sp => sp.GetRequiredService<LearningDbContext>()),
        (DatabaseStartupModuleNames.AiGateway, sp => sp.GetRequiredService<AiGatewayDbContext>()),
        (DatabaseStartupModuleNames.Quota, sp => sp.GetRequiredService<QuotaDbContext>()),
        (DatabaseStartupModuleNames.Practice, sp => sp.GetRequiredService<PracticeDbContext>()),
        (DatabaseStartupModuleNames.Feedback, sp => sp.GetRequiredService<FeedbackDbContext>()),
        ("Progress", sp => sp.GetRequiredService<ProgressDbContext>())
    ];

    foreach (var module in modules)
    {
        if (!options.ShouldApplyMigrations(module.Name))
        {
            logger.LogInformation(
                "{Module} migration skipped on startup. Apply explicitly with dotnet ef database update for its DbContext.",
                module.Name);
            continue;
        }

        await module.Resolve(serviceProvider).Database.MigrateAsync();
        logger.LogInformation("{Module} migration applied on startup.", module.Name);
    }
}

public partial class Program { }
