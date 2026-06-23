using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.Identity.Infrastructure.Persistence;
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
            var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
            var auditDb = scope.ServiceProvider.GetRequiredService<AuditDbContext>();

            var pendingIdentity = identityDb.Database.IsRelational()
                ? (await identityDb.Database.GetPendingMigrationsAsync(cancellationToken)).ToList()
                : new List<string>();
            var pendingAudit = auditDb.Database.IsRelational()
                ? (await auditDb.Database.GetPendingMigrationsAsync(cancellationToken)).ToList()
                : new List<string>();

            bool hasMismatch = false;

            if (pendingIdentity.Any())
            {
                _logger.LogCritical("Startup aborted: IdentityDbContext has pending migrations: {Migrations}", string.Join(", ", pendingIdentity));
                hasMismatch = true;
            }

            if (pendingAudit.Any())
            {
                _logger.LogCritical("Startup aborted: AuditDbContext has pending migrations: {Migrations}", string.Join(", ", pendingAudit));
                hasMismatch = true;
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
