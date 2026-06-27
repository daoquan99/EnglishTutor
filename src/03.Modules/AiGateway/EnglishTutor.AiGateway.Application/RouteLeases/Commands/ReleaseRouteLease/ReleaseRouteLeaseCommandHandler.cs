using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.ReleaseRouteLease;

internal sealed class ReleaseRouteLeaseCommandHandler : ICommandHandler<ReleaseRouteLeaseCommand, ReleaseRouteLeaseResult>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;

    public ReleaseRouteLeaseCommandHandler(IAiGatewayUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ReleaseRouteLeaseResult>> Handle(ReleaseRouteLeaseCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var lease = await _unitOfWork.RouteLeases.GetByIdAsync(request.LeaseId, ct);
        if (lease is null)
        {
            return Result.Success(new ReleaseRouteLeaseResult { Status = ReleaseRouteLeaseStatus.LeaseNotFound, ErrorCode = "aigateway.release.not_found" });
        }

        if (lease.Status != AiRouteLeaseStatus.Reserved)
        {
            return Result.Success(new ReleaseRouteLeaseResult { Status = ReleaseRouteLeaseStatus.InvalidLeaseState, ErrorCode = "aigateway.release.invalid_state" });
        }

        lease.Release();
        _unitOfWork.RouteLeases.Update(lease);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new ReleaseRouteLeaseResult { Status = ReleaseRouteLeaseStatus.Success });
    }
}
