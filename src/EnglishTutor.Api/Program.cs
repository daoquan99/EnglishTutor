using EnglishTutor.Audit.Infrastructure;
using EnglishTutor.BuildingBlocks.Infrastructure.Correlation;
using EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Application;
using EnglishTutor.Identity.Presentation;
using EnglishTutor.Audit.Presentation;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Learning.Infrastructure;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Learning.Presentation;
using EnglishTutor.Quota.Infrastructure.Extensions;
using EnglishTutor.AiGateway.Infrastructure.Extensions;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MassTransit;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration));

// Strongly-typed options + startup validation
builder.Services.AddBaseOptions(builder.Configuration);

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

// Configure MassTransit EF outbox on API host with in-memory bus stub.
// This supports transactional outbox writing in HTTP handlers without a broker link.
builder.Services.AddApiOutboxMessaging(x =>
{
    x.AddEntityFrameworkOutbox<IdentityDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddEntityFrameworkOutbox<AuditDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddEntityFrameworkOutbox<AiGatewayDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
    x.AddEntityFrameworkOutbox<LearningDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    var auditDb = scope.ServiceProvider.GetRequiredService<EnglishTutor.Audit.Infrastructure.Persistence.AuditDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var applyAuditMigrationsOnStartup = builder.Configuration
        .GetValue<bool>("Database:ApplyAuditMigrationsOnStartup");
    try
    {
        await db.Database.MigrateAsync();
        if (applyAuditMigrationsOnStartup)
        {
            await auditDb.Database.MigrateAsync();
        }
        else
        {
            logger.LogInformation(
                "Audit migration skipped on startup (Database:ApplyAuditMigrationsOnStartup=false). " +
                "Apply explicitly with: dotnet ef database update --context AuditDbContext");
        }
        await scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>().SeedAsync();
    }
    catch (Npgsql.NpgsqlException)
    {
        // DB not reachable — skip.
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup seeding failed.");
    }
}

// Middleware
app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();

// Authentication + Authorization — required because /api/me and
// /api/auth/logout carry .RequireAuthorization() metadata.
app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

// Health Checks
app.MapBaseHealthChecks();

// Identity endpoints
app.MapIdentityEndpoints();
app.MapAuditEndpoints();
app.MapLearningEndpoints();

// Minimal API root
app.MapGet("/", () => Results.Ok(new
{
    service = "EnglishTutor.Api",
    version = "1.0.0",
    status = "Running",
    timestamp = DateTime.UtcNow
}))
.WithName("GetRoot")
.ExcludeFromDescription();

app.Run();

public partial class Program { }
