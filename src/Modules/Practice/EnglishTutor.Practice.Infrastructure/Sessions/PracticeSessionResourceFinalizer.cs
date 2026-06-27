using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Contracts;
using EnglishTutor.Quota.Contracts.Dtos;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;
using Microsoft.Extensions.Logging;
using EnglishTutor.Practice.Application.Sessions.Abstractions;

namespace EnglishTutor.Practice.Infrastructure.Sessions;

public sealed class PracticeSessionResourceFinalizer : IPracticeSessionResourceFinalizer
{
    private readonly IQuotaModule _quota;
    private readonly IAiGatewayModule _aiGateway;
    private readonly ILogger<PracticeSessionResourceFinalizer> _logger;

    public PracticeSessionResourceFinalizer(
        IQuotaModule quota,
        IAiGatewayModule aiGateway,
        ILogger<PracticeSessionResourceFinalizer> logger)
    {
        _quota = quota;
        _aiGateway = aiGateway;
        _logger = logger;
    }

    public async Task FinalizeResourcesAsync(
        Guid reservationId,
        Guid leaseId,
        bool isExpired,
        CancellationToken ct,
        int? actualMinutes = null)
    {
        if (isExpired)
        {
            // Cancel reservation (best-effort, log safe warning if fails)
            if (reservationId != Guid.Empty)
            {
                try
                {
                    var result = await _quota.CancelReservationAsync(new CancelReservationRequest { ReservationId = reservationId }, ct);
                    if (result.Status != CancelReservationStatus.Success)
                    {
                        _logger.LogWarning("Quota reservation cancellation returned status {Status} for ReservationId: {ReservationId}", result.Status, reservationId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cancel Quota reservation for ReservationId: {ReservationId}", reservationId);
                }
            }

            // Release AI route lease (best-effort, log safe warning if fails)
            if (leaseId != Guid.Empty)
            {
                try
                {
                    var result = await _aiGateway.ReleaseRouteLeaseAsync(new ReleaseRouteLeaseRequest { LeaseId = leaseId }, ct);
                    if (result.Status != ReleaseRouteLeaseStatus.Success)
                    {
                        _logger.LogWarning("AiGateway route lease release returned status {Status} for LeaseId: {LeaseId}", result.Status, leaseId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to release AiGateway route lease for LeaseId: {LeaseId}", leaseId);
                }
            }
        }
        else
        {
            // Confirm quota usage (best-effort, log safe warning if fails)
            if (reservationId != Guid.Empty)
            {
                try
                {
                    var result = await _quota.ConfirmSessionUsageAsync(new ConfirmSessionUsageRequest
                    {
                        ReservationId = reservationId,
                        ActualMinutesUsed = actualMinutes ?? 1
                    }, ct);
                    if (result.Status != ConfirmSessionUsageStatus.Success)
                    {
                        _logger.LogWarning("Quota usage confirmation returned status {Status} for ReservationId: {ReservationId}", result.Status, reservationId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to confirm Quota usage for ReservationId: {ReservationId}", reservationId);
                }
            }

            // Confirm AI route usage (best-effort, log safe warning if fails)
            if (leaseId != Guid.Empty)
            {
                try
                {
                    var result = await _aiGateway.ConfirmRouteUsageAsync(new ConfirmRouteUsageRequest { LeaseId = leaseId }, ct);
                    if (result.Status != ConfirmRouteUsageStatus.Success)
                    {
                        _logger.LogWarning("AiGateway route usage confirmation returned status {Status} for LeaseId: {LeaseId}", result.Status, leaseId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to confirm AiGateway route usage for LeaseId: {LeaseId}", leaseId);
                }
            }
        }
    }
}
