using System;

namespace EnglishTutor.AiGateway.Application;

/// <summary>
/// Strong-typed options for the AI Gateway module.
/// </summary>
public class AiGatewayOptions
{
    public const string SectionName = "AiGateway";

    /// <summary>
    /// Default Time-To-Live for route leases in minutes.
    /// </summary>
    public int DefaultLeaseTtlMinutes { get; set; } = 10;

    /// <summary>
    /// Master symmetric encryption key used to encrypt/decrypt provider API keys.
    /// </summary>
    public string EncryptionMasterKey { get; set; } = default!;

    /// <summary>
    /// Maximum retries for database concurrency issues.
    /// </summary>
    public int MaxConcurrencyRetryCount { get; set; } = 3;

    /// <summary>
    /// Cooldown period for rate-limited keys in minutes.
    /// </summary>
    public int KeyCooldownMinutes { get; set; } = 5;

    /// <summary>
    /// Interval in seconds to clean up expired route leases.
    /// </summary>
    public int ExpiredLeaseCleanupIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Batch size for cleaning up expired leases.
    /// </summary>
    public int ExpiredLeaseCleanupBatchSize { get; set; } = 20;
}
