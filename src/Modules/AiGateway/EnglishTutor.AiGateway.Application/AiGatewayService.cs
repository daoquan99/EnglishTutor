using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;

namespace EnglishTutor.AiGateway.Application;

/// <summary>
/// Service implementation of the IAiGatewayModule contract.
/// </summary>
public class AiGatewayService : IAiGatewayModule
{
    public Task<CreateRouteLeaseResult> CreateRouteLeaseAsync(CreateRouteLeaseRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<ConfirmRouteUsageResult> ConfirmRouteUsageAsync(ConfirmRouteUsageRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<ReleaseRouteLeaseResult> ReleaseRouteLeaseAsync(ReleaseRouteLeaseRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<ExecuteChatCompletionResult> ExecuteChatCompletionAsync(ExecuteChatCompletionRequest request, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
