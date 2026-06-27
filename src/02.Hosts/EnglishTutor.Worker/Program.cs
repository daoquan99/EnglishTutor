using EnglishTutor.AiGateway.Infrastructure.Extensions;
using EnglishTutor.AiGateway.Infrastructure.Persistence;
using EnglishTutor.Audit.Infrastructure;
using EnglishTutor.Audit.Infrastructure.Messaging;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;
using EnglishTutor.BuildingBlocks.Infrastructure.Messaging;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.Identity.Application;
using EnglishTutor.Identity.Infrastructure;
using EnglishTutor.Identity.Infrastructure.Messaging;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Learning.Infrastructure;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Quota.Infrastructure.Extensions;
using EnglishTutor.Feedback.Infrastructure.Extensions;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using EnglishTutor.Feedback.Infrastructure.Messaging;
using EnglishTutor.Practice.Infrastructure.Extensions;
using EnglishTutor.Practice.Infrastructure.Persistence;
using EnglishTutor.Worker.HostedServices;
using EnglishTutor.Worker.Jobs;
using EnglishTutor.Worker.Options;
using EnglishTutor.Worker.Readiness;
using MassTransit;
using Serilog;

RepositoryEnvironment.LoadIntoProcess();

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSerilog(loggerConfig =>
    loggerConfig.ReadFrom.Configuration(builder.Configuration));

// Strongly-typed options + startup validation
builder.Services.AddBaseOptions(builder.Configuration);
builder.Services.AddRouting();
builder.Services.AddScoped<ICurrentUser, WorkerCurrentUser>();

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
    .Validate(o => o.QuotaReservationExpiryInterval > TimeSpan.Zero,
        "Worker:QuotaReservationExpiryInterval must be greater than zero.")
    .Validate(o => o.QuotaReservationExpiryBatchSize > 0,
        "Worker:QuotaReservationExpiryBatchSize must be greater than zero.")
    .Validate(o => o.AiRouteLeaseExpiryInterval > TimeSpan.Zero,
        "Worker:AiRouteLeaseExpiryInterval must be greater than zero.")
    .Validate(o => o.AiRouteLeaseExpiryBatchSize > 0,
        "Worker:AiRouteLeaseExpiryBatchSize must be greater than zero.")
    .Validate(o => o.UsageAggregationInterval > TimeSpan.Zero,
        "Worker:UsageAggregationInterval must be greater than zero.")
    .Validate(o => o.KeyCooldownReleaseInterval > TimeSpan.Zero,
        "Worker:KeyCooldownReleaseInterval must be greater than zero.")
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
builder.Services.AddPracticeModule(builder.Configuration);
builder.Services.AddFeedbackModule(builder.Configuration);

// Schema readiness and background jobs hosted services
builder.Services.AddHostedService<WorkerSchemaReadinessHostedService>();
builder.Services.AddHostedService<ExpiredRefreshTokensCleanupHostedService>();
builder.Services.AddHostedService<QuotaExpiredReservationCleanupHostedService>();
builder.Services.AddHostedService<ExpireAiRouteLeasesHostedService>();

builder.Services.AddWorkerBrokerMessaging<IIdentityBus>(
    builder.Configuration,
    x =>
    {
        x.AddEntityFrameworkOutbox<AuditDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
        x.AddEntityFrameworkOutbox<LearningDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
        x.AddEntityFrameworkOutbox<PracticeDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
        x.AddEntityFrameworkOutbox<FeedbackDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });
        x.AddFeedbackConsumers();
    },
    x =>
    {
        x.AddEntityFrameworkOutbox<IIdentityBus, IdentityDbContext>(o =>
        {
            o.UsePostgres();
            o.UseBusOutbox();
        });

        x.AddAuditSecurityEventConsumers();
    });

builder.Services.AddScoped<MassTransit.EntityFrameworkCoreIntegration.EntityFrameworkScopedBusContext<MassTransit.IBus, EnglishTutor.Learning.Infrastructure.Persistence.LearningDbContext>>();
builder.Services.AddScoped<MassTransit.EntityFrameworkCoreIntegration.EntityFrameworkScopedBusContext<MassTransit.IBus, EnglishTutor.Practice.Infrastructure.Persistence.PracticeDbContext>>();
builder.Services.AddScoped<MassTransit.EntityFrameworkCoreIntegration.EntityFrameworkScopedBusContext<MassTransit.IBus, EnglishTutor.Feedback.Infrastructure.Persistence.FeedbackDbContext>>();
builder.Services.AddScoped<MassTransit.EntityFrameworkCoreIntegration.EntityFrameworkScopedBusContext<EnglishTutor.Identity.Infrastructure.Messaging.IIdentityBus, EnglishTutor.Identity.Infrastructure.Persistence.IdentityDbContext>>();

var host = builder.Build();

Log.Information("EnglishTutor.Worker starting...");

host.Run();

internal sealed class WorkerCurrentUser : ICurrentUser
{
    public Guid? UserId => null;
    public string? Email => null;
    public bool IsAuthenticated => false;
    public IReadOnlyList<string> Roles => [];
    public bool IsInRole(string role) => false;
}
