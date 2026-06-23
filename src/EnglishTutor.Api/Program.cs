using EnglishTutor.Audit.Infrastructure;
using EnglishTutor.BuildingBlocks.Infrastructure.Correlation;
using EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using EnglishTutor.BuildingBlocks.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Identity.Application;
using EnglishTutor.Identity.Presentation;
using Microsoft.EntityFrameworkCore;
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

var app = builder.Build();

// Apply EF Core migrations + seed Owner on startup.
// Identity migration/seeding runs unconditionally (established in Task 23).
// Audit migration is GATED by `Database:ApplyAuditMigrationsOnStartup`
// (default false) so it must be applied explicitly via
// `dotnet ef database update --context AuditDbContext` after approval,
// not as a side-effect of API startup.
// Skipped silently if the database is unreachable (dev/CI without Docker).
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

// Health Checks
app.MapBaseHealthChecks();

// Identity endpoints
app.MapIdentityEndpoints();

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
