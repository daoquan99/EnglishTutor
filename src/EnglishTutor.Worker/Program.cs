using EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;
using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Serilog
builder.Services.AddSerilog(loggerConfig =>
    loggerConfig.ReadFrom.Configuration(builder.Configuration));

// Strongly-typed options + startup validation
builder.Services.AddBaseOptions(builder.Configuration);

// Health checks
builder.Services.AddBaseHealthChecks();
builder.Services.AddInfrastructureHealthChecks();

// Active readiness probe at startup — aborts the host if any infrastructure
// dependency (Postgres/RabbitMQ/Redis) is unreachable. The API exposes
// /health/ready instead, so it does not register this.
builder.Services.AddStartupReadinessProbe();

// Add background services from modules
// TODO: Register module consumers/jobs as modules are implemented

var host = builder.Build();

Log.Information("EnglishTutor.Worker starting...");

host.Run();
