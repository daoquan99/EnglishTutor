using EnglishTutor.AiGateway.Infrastructure.Persistence;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.Feedback.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Learning.Infrastructure.Persistence;
using EnglishTutor.Practice.Infrastructure.Persistence;
using EnglishTutor.Quota.Infrastructure.Persistence;
using EnglishTutor.Worker.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Worker.Readiness;

/// <summary>
/// A Worker-specific hosted service that checks for pending migrations at startup.
/// Exiting startup early ensures that the Worker does not process queue messages against
/// an out-of-date database schema.
/// </summary>
public sealed class WorkerSchemaReadinessHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<WorkerSchemaReadinessHostedService> _logger;
    private readonly WorkerOptions _options;

    public WorkerSchemaReadinessHostedService(
        IServiceProvider serviceProvider,
        IHostApplicationLifetime lifetime,
        ILogger<WorkerSchemaReadinessHostedService> logger,
        IOptions<WorkerOptions> options)
    {
        _serviceProvider = serviceProvider;
        _lifetime = lifetime;
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.RequireSchemaMatchOnStartup)
        {
            _logger.LogWarning("Worker schema readiness check is disabled (Worker:RequireSchemaMatchOnStartup = false).");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        try
        {
            bool hasMismatch = false;

            (string Name, DbContext DbContext)[] modules =
            [
                ("Identity", scope.ServiceProvider.GetRequiredService<IdentityDbContext>()),
                ("Audit", scope.ServiceProvider.GetRequiredService<AuditDbContext>()),
                ("Learning", scope.ServiceProvider.GetRequiredService<LearningDbContext>()),
                ("AiGateway", scope.ServiceProvider.GetRequiredService<AiGatewayDbContext>()),
                ("Quota", scope.ServiceProvider.GetRequiredService<QuotaDbContext>()),
                ("Practice", scope.ServiceProvider.GetRequiredService<PracticeDbContext>()),
                ("Feedback", scope.ServiceProvider.GetRequiredService<FeedbackDbContext>())
            ];

            foreach (var module in modules)
            {
                var pendingMigrations = module.DbContext.Database.IsRelational()
                    ? (await module.DbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList()
                    : [];

                if (pendingMigrations.Count > 0)
                {
                    _logger.LogCritical(
                        "Startup aborted: {Module} DbContext has pending migrations: {Migrations}",
                        module.Name,
                        string.Join(", ", pendingMigrations));
                    hasMismatch = true;
                }
            }

            if (hasMismatch)
            {
                Environment.ExitCode = 1;
                _lifetime.StopApplication();
            }
            else
            {
                _logger.LogInformation("Worker schema readiness check: OK (No pending migrations found).");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to perform database schema readiness check. Aborting startup.");
            Environment.ExitCode = 1;
            _lifetime.StopApplication();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
