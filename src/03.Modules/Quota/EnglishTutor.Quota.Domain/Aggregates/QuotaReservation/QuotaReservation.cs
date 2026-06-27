using System;
using EnglishTutor.BuildingBlocks.Domain.Aggregates;

namespace EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;

/// <summary>
/// Represents a quota reservation.
/// </summary>
public class QuotaReservation : AggregateRoot
{
    public enum ReservationStatus
    {
        Reserved,
        Confirmed,
        Cancelled,
        Expired
    }

    public Guid UserId { get; private set; }
    public string IdempotencyKey { get; private set; } = default!;
    public int RequestedMinutes { get; private set; }
    public ReservationStatus Status { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime QuotaDate { get; private set; } // Date only, stored as DateTime with time 00:00:00
    public DateTime? ConfirmedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public Guid? CorrelationId { get; private set; }
    public Guid? PracticeSessionId { get; private set; }
    public long Version { get; private set; }

    private QuotaReservation() { }

    public static QuotaReservation Create(
        Guid id,
        Guid userId,
        string idempotencyKey,
        int requestedMinutes,
        DateTime expiresAtUtc,
        DateTime quotaDate,
        Guid? correlationId = null,
        Guid? practiceSessionId = null)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new ArgumentException("IdempotencyKey is required.", nameof(idempotencyKey));
        if (requestedMinutes <= 0)
            throw new ArgumentOutOfRangeException(nameof(requestedMinutes));
        if (expiresAtUtc == DateTime.MinValue)
            throw new ArgumentException("ExpiresAtUtc required.", nameof(expiresAtUtc));
        if (quotaDate == DateTime.MinValue)
            throw new ArgumentException("QuotaDate required.", nameof(quotaDate));

        return new QuotaReservation
        {
            Id = id,
            UserId = userId,
            IdempotencyKey = idempotencyKey,
            RequestedMinutes = requestedMinutes,
            Status = ReservationStatus.Reserved,
            ExpiresAtUtc = expiresAtUtc,
            QuotaDate = quotaDate,
            CorrelationId = correlationId,
            PracticeSessionId = practiceSessionId,
            Version = 1 // Initial version
        };
    }

    public void Confirm(DateTime confirmedAtUtc)
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException("Only reserved reservations can be confirmed.");
        Status = ReservationStatus.Confirmed;
        ConfirmedAtUtc = confirmedAtUtc;
        Version++;
    }

    public void Cancel(DateTime cancelledAtUtc)
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException("Only reserved reservations can be cancelled.");
        Status = ReservationStatus.Cancelled;
        CancelledAtUtc = cancelledAtUtc;
        Version++;
    }

    public void Expire(DateTime expiredAtUtc)
    {
        if (Status != ReservationStatus.Reserved)
            throw new InvalidOperationException("Only reserved reservations can be expired.");
        Status = ReservationStatus.Expired;
        // Note: We don't set CancelledAtUtc for expiration
        Version++;
    }
}