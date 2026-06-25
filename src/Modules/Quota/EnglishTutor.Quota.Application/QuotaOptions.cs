namespace EnglishTutor.Quota.Application;

/// <summary>
/// Options for the Quota module.
/// </summary>
public class QuotaOptions
{
    /// <summary>
    /// Default daily max session minutes if no rule is found.
    /// </summary>
    public int DefaultDailyMaxSessionMinutes { get; set; } = 60;

    /// <summary>
    /// Default daily max sessions if no rule is found.
    /// </summary>
    public int DefaultDailyMaxSessions { get; set; } = 5;

    /// <summary>
    /// Default max single session minutes if no rule is found.
    /// </>
    public int DefaultMaxSingleSessionMinutes { get; set; } = 30;

    /// <summary>
    /// Reservation TTL in minutes.
    /// </summary>
    public int ReservationTtlMinutes { get; set; } = 30;

    /// <summary>
    /// Interval in seconds for the expired reservation cleanup worker.
    /// </summary>
    public int ExpiredReservationCleanupIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Batch size for the expired reservation cleanup worker.
    /// </summary>
    public int ExpiredReservationCleanupBatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum number of concurrency retries.
    /// </summary>
    public int MaxConcurrencyRetryCount { get; set; } = 3;
}