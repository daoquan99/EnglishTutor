using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ConfirmRouteUsage;

internal sealed class ConfirmRouteUsageCommandHandler : ICommandHandler<ConfirmRouteUsageCommand, ConfirmRouteUsageResult>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;

    public ConfirmRouteUsageCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<Result<ConfirmRouteUsageResult>> Handle(ConfirmRouteUsageCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var lease = await _unitOfWork.RouteLeases.GetByIdAsync(request.LeaseId, ct);
        if (lease is null)
        {
            return Result.Success(new ConfirmRouteUsageResult { Status = ConfirmRouteUsageStatus.LeaseNotFound, ErrorCode = "aigateway.confirm.not_found" });
        }

        if (lease.Status == AiRouteLeaseStatus.Reserved && lease.ExpiryAtUtc < _clock.UtcNow)
        {
            return Result.Success(new ConfirmRouteUsageResult { Status = ConfirmRouteUsageStatus.LeaseExpired, ErrorCode = "aigateway.confirm.expired" });
        }

        if (lease.Status != AiRouteLeaseStatus.Reserved)
        {
            return Result.Success(new ConfirmRouteUsageResult { Status = ConfirmRouteUsageStatus.InvalidLeaseState, ErrorCode = "aigateway.confirm.invalid_state" });
        }

        lease.Confirm();
        _unitOfWork.RouteLeases.Update(lease);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new ConfirmRouteUsageResult { Status = ConfirmRouteUsageStatus.Success });
    }
}
