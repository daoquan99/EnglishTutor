using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Hosted service that actively probes infrastructure dependencies at startup.
/// Unlike the API (which exposes <c>/health/ready</c>), the Worker has no
/// HTTP endpoint, so it must run the readiness checks itself before
/// declaring the host started.
/// </summary>
/// <remarks>
/// <para><b>Failure policy:</b> graceful shutdown. If any <c>ready</c>-tagged
/// check returns Unhealthy, the host is asked to stop with exit code 1 and
/// a clear one-line summary is logged. This guarantees that a Worker can
/// never accept work against unreachable Postgres/RabbitMQ/Redis.</para>
/// <para><b>Default timeout:</b> 60 seconds — sized for cold-start TLS
/// handshake and managed-DB warm-up. Override via the constructor
/// <c>timeout</c> parameter or <see cref="HealthChecksHostBuilderExtensions.AddStartupReadinessProbe"/>.</para>
/// </remarks>
public sealed class StartupReadinessProbe : IHostedService
{
    /// <summary>Default probe timeout: 60 seconds.</summary>
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(60);

    private readonly IServiceProvider _services;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<StartupReadinessProbe> _logger;
    private readonly TimeSpan _timeout;

    public StartupReadinessProbe(
        IServiceProvider services,
        IHostApplicationLifetime lifetime,
        ILogger<StartupReadinessProbe> logger,
        TimeSpan? timeout = null)
    {
        _services = services;
        _lifetime = lifetime;
        _logger = logger;
        _timeout = timeout ?? DefaultTimeout;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_timeout);

        using var scope = _services.CreateScope();
        var healthCheckService = scope.ServiceProvider.GetRequiredService<HealthCheckService>();

        HealthReport report;
        try
        {
            report = await healthCheckService.CheckHealthAsync(
                check => check.Tags.Contains("ready"),
                cts.Token);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogCritical(
                "Startup readiness probe TIMED OUT after {Timeout}s. Aborting host startup. " +
                "Increase probe timeout if your infrastructure is slow to accept connections.",
                _timeout.TotalSeconds);
            Environment.ExitCode = 1;
            _lifetime.StopApplication();
            return;
        }

        if (report.Status == HealthStatus.Healthy)
        {
            _logger.LogInformation(
                "Startup readiness probe: OK ({CheckCount} checks passed in {Duration}ms)",
                report.Entries.Count,
                report.TotalDuration.TotalMilliseconds);
            return;
        }

        var failed = report.Entries
            .Where(e => e.Value.Status != HealthStatus.Healthy)
            .Select(e => $"{e.Key}={e.Value.Status}")
            .ToList();

        // Single one-line summary, then graceful shutdown.
        // We do NOT throw because that surfaces as an unhandled stacktrace;
        // asking the host to stop cleanly is friendlier to orchestrators.
        _logger.LogCritical(
            "Startup readiness probe FAILED. Failing checks: {Failed}. Aborting host startup.",
            string.Join(", ", failed));

        Environment.ExitCode = 1;
        _lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
