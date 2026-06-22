using EnglishTutor.BuildingBlocks.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;

/// <summary>
/// Adds readiness health checks for the PostgreSQL, RabbitMQ, and Redis
/// infrastructure dependencies. Each check is tagged "ready" so the
/// <c>/health/ready</c> endpoint reports on dependency status.
/// </summary>
public static class InfrastructureHealthChecks
{
    public static IServiceCollection AddInfrastructureHealthChecks(this IServiceCollection services)
    {
        // Resolve options once to extract connection strings.
        // The checks themselves also re-resolve at execution time via DI.
        services
            .AddHealthChecks()
            .AddPostgresHealthCheck()
            .AddRabbitMqHealthCheck()
            .AddRedisHealthCheck();

        return services;
    }

    /// <summary>
    /// Registers <see cref="StartupReadinessProbe"/> as a hosted service so
    /// the host aborts startup when Postgres/RabbitMQ/Redis are unreachable.
    /// Recommended for worker hosts that have no HTTP readiness endpoint.
    /// </summary>
    public static IServiceCollection AddStartupReadinessProbe(
        this IServiceCollection services,
        TimeSpan? timeout = null)
    {
        services.AddHostedService(sp =>
            new StartupReadinessProbe(
                sp,
                sp.GetRequiredService<Microsoft.Extensions.Hosting.IHostApplicationLifetime>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<StartupReadinessProbe>>(),
                timeout));

        return services;
    }

    private static IHealthChecksBuilder AddPostgresHealthCheck(this IHealthChecksBuilder builder)
    {
        // AddCheck returns the existing builder; we resolve options inside a closure check.
        return builder.Add(new HealthCheckRegistration(
            name: "postgres",
            factory: sp => new DelegateHealthCheck(async ct =>
            {
                var options = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                return await PostgresPingAsync(options.Default, ct);
            }),
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready", "db" }));
    }

    private static IHealthChecksBuilder AddRabbitMqHealthCheck(this IHealthChecksBuilder builder)
    {
        return builder.Add(new HealthCheckRegistration(
            name: "rabbitmq",
            factory: sp => new DelegateHealthCheck(async ct =>
            {
                var options = sp.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
                return await RabbitMqPingAsync(options, ct);
            }),
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready", "broker" }));
    }

    private static IHealthChecksBuilder AddRedisHealthCheck(this IHealthChecksBuilder builder)
    {
        return builder.Add(new HealthCheckRegistration(
            name: "redis",
            factory: sp => new DelegateHealthCheck(async ct =>
            {
                var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
                return await RedisPingAsync(options.Configuration, ct);
            }),
            failureStatus: HealthStatus.Unhealthy,
            tags: new[] { "ready", "cache" }));
    }

    // ---- Concrete ping implementations ----

    private static async Task<HealthCheckResult> PostgresPingAsync(
        string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            await using var conn = new Npgsql.NpgsqlConnection(connectionString);
            await conn.OpenAsync(cancellationToken);
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT 1";
            cmd.CommandTimeout = 5;
            var result = await cmd.ExecuteScalarAsync(cancellationToken);
            return result is not null
                ? HealthCheckResult.Healthy("Postgres reachable")
                : HealthCheckResult.Unhealthy("Postgres returned null");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Postgres unreachable", ex);
        }
    }

    private static async Task<HealthCheckResult> RabbitMqPingAsync(
        RabbitMqOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                HostName = options.HostName,
                Port = options.Port,
                UserName = options.UserName,
                Password = options.Password,
                VirtualHost = options.VirtualHost,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(5),
                SocketReadTimeout = TimeSpan.FromSeconds(5),
                SocketWriteTimeout = TimeSpan.FromSeconds(5)
            };

            // Race the connection attempt against the cancellation token so
            // that a stuck socket cannot outlive the probe's budget. If the
            // token fires first we surface a clean Unhealthy instead of
            // hanging the host startup.
            var connectionTask = factory.CreateConnectionAsync(cancellationToken);
            var timeoutTask = Task.Delay(Timeout.Infinite, cancellationToken);
            var winner = await Task.WhenAny(connectionTask, timeoutTask);

            if (winner != connectionTask)
            {
                return HealthCheckResult.Unhealthy("RabbitMQ connection attempt cancelled or timed out");
            }

            using var connection = await connectionTask;
            return connection.IsOpen
                ? HealthCheckResult.Healthy("RabbitMQ reachable")
                : HealthCheckResult.Unhealthy("RabbitMQ connection not open");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ connection cancelled");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ unreachable", ex);
        }
    }

    private static async Task<HealthCheckResult> RedisPingAsync(
        string configuration, CancellationToken cancellationToken)
    {
        try
        {
            using var mux = await StackExchange.Redis.ConnectionMultiplexer.ConnectAsync(configuration);
            var db = mux.GetDatabase();
            var pong = await db.PingAsync();
            return HealthCheckResult.Healthy(
                description: $"Redis reachable ({pong.TotalMilliseconds:F1}ms)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis unreachable", ex);
        }
    }

    /// <summary>
    /// Minimal <see cref="IHealthCheck"/> that delegates to a <see cref="Func{T,TResult}"/>.
    /// </summary>
    private sealed class DelegateHealthCheck : IHealthCheck
    {
        private readonly Func<CancellationToken, Task<HealthCheckResult>> _check;

        public DelegateHealthCheck(Func<CancellationToken, Task<HealthCheckResult>> check)
        {
            _check = check;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _check(cancellationToken);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Health check threw an exception", ex);
            }
        }
    }
}
