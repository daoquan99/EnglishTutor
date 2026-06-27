using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EnglishTutor.Quota.Contracts;
using EnglishTutor.Quota.Contracts.Dtos;
using EnglishTutor.Quota.Application.Reservations.Commands.ReserveQuota;
using EnglishTutor.Quota.Application.Reservations.Commands.ConfirmSessionUsage;
using EnglishTutor.Quota.Application.Reservations.Commands.CancelReservation;

namespace EnglishTutor.Quota.Application;

public sealed class QuotaService : IQuotaModule
{
    private readonly ISender _sender;

    public QuotaService(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public async Task<ReserveSessionQuotaResult> ReserveSessionQuotaAsync(
        ReserveSessionQuotaRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new ReserveQuotaCommand(request), ct);
        return result.Value!;
    }

    public async Task<ConfirmSessionUsageResult> ConfirmSessionUsageAsync(
        ConfirmSessionUsageRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new ConfirmSessionUsageCommand(request), ct);
        return result.Value!;
    }

    public async Task<CancelReservationResult> CancelReservationAsync(
        CancelReservationRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new CancelReservationCommand(request), ct);
        return result.Value!;
    }
}