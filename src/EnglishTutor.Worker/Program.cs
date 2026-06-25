using EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using EnglishTutor.Identity.Application;
using EnglishTutor.Identity.Infrastructure;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Audit.Infrastructure;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.Worker.Jobs;
using EnglishTutor.Worker.Options;
using EnglishTutor.Worker.Readiness;
using EnglishTutor.Learning.Infrastructure;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Quota.Infrastructure.Extensions;
using EnglishTutor.AiGateway.Infrastructure.Extensions;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using EnglishTutor.Worker.HostedServices;
using MassTransit;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Serilog
builder.Services.AddSerilog(loggerConfig =>
    loggerConfig.ReadFrom.Configuration(builder.Configuration));

// Strongly-typed options + startup validation
builder.Services.AddBaseOptions(builder.Configuration);

builder.Services
    .AddOptions<WorkerOptions>()
    .Bind(builder.Configuration.GetSection(WorkerOptions.SectionName))
    .ValidateDataAnnotations()
    .Validate(o => o.RefreshTokenCleanupInterval > TimeSpan.Zero, 
        "Worker:RefreshTokenCleanupInterval must be greater than zero.")
    .Validate(o => o.RefreshTokenRetentionDays >= 1 && o.RefreshTokenRetentionDays <= 365, 
        "Worker:RefreshTokenRetentionDays must be between 1 and 365 days.")
    .Validate(o => o.ShutdownTimeout > TimeSpan.Zero,
        "Worker:ShutdownTimeout must be greater than zero.")
    .ValidateOnStart();

// Health checks
builder.Services.AddBaseHealthChecks();
builder.Services.AddInfrastructureHealthChecks();

// Active readiness probe at startup — aborts the host if any infrastructure
// dependency (Postgres/RabbitMQ/Redis) is unreachable. The API exposes
// /health/ready instead, so it does not register this.
builder.Services.AddStartupReadinessProbe();

// Register DbContexts and services from modules
builder.Services.AddIdentityApplication(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddAudit(builder.Configuration);
builder.Services.AddLearningInfrastructure(builder.Configuration);
builder.Services.AddQuotaModule(builder.Configuration);
builder.Services.AddAiGatewayModule(builder.Configuration);

// Schema readiness and background jobs hosted services
builder.Services.AddHostedService<WorkerSchemaReadinessHostedService>();
builder.Services.AddHostedService<ExpiredRefreshTokensCleanupHostedService>();
builder.Services.AddHostedService<QuotaExpiredReservationCleanupHostedService>();

// Configure MassTransit on Worker with RabbitMQ and EF outboxes
builder.Services.AddWorkerBrokerMessaging(builder.Configuration, x =>
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

var host = builder.Build();

Log.Information("EnglishTutor.Worker starting...");

host.Run();
