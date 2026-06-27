using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.Worker.Options;

/// <summary>
/// Strongly-typed options for the Worker host background jobs and lifecycle settings.
/// Bound from configuration section <c>Worker</c>.
/// </summary>
public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    /// <summary>
    /// Whether background scheduled cleanup and maintenance jobs are enabled on this host instance.
    /// Default is true.
    /// </summary>
    public bool JobsEnabled { get; set; } = true;

    /// <summary>
    /// How often the expired refresh token cleanup job executes.
    /// Default is 24 hours.
    /// </summary>
    public TimeSpan RefreshTokenCleanupInterval { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Number of days to retain expired refresh tokens for audit and forensics.
    /// Default is 30 days.
    /// </summary>
    public int RefreshTokenRetentionDays { get; set; } = 30;

    /// <summary>
    /// If true, the worker verifies that the database schema matches the expected model (no pending migrations)
    /// and fails fast on startup if a mismatch is detected.
    /// Default is true.
    /// </summary>
    public bool RequireSchemaMatchOnStartup { get; set; } = true;

    /// <summary>
    /// Graceful shutdown timeout allowed for MassTransit and background services to finish processing active work.
    /// Default is 30 seconds.
    /// </summary>
    public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(30);

    public bool EnableQuotaReservationExpiry { get; set; } = true;
    public TimeSpan QuotaReservationExpiryInterval { get; set; } = TimeSpan.FromMinutes(5);
    public int QuotaReservationExpiryBatchSize { get; set; } = 100;

    public bool EnableAiRouteLeaseExpiry { get; set; } = true;
    public TimeSpan AiRouteLeaseExpiryInterval { get; set; } = TimeSpan.FromMinutes(5);
    public int AiRouteLeaseExpiryBatchSize { get; set; } = 100;

    public bool EnableExpiredRefreshTokenCleanup { get; set; } = true;

    public bool EnableUsageAggregation { get; set; } = false;
    public TimeSpan UsageAggregationInterval { get; set; } = TimeSpan.FromHours(1);

    public bool EnableKeyCooldownRelease { get; set; } = false;
    public TimeSpan KeyCooldownReleaseInterval { get; set; } = TimeSpan.FromMinutes(5);
}
