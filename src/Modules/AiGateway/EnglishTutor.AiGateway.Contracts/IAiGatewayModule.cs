using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Contracts.Dtos;

namespace EnglishTutor.AiGateway.Contracts;

/// <summary>
/// Contract interface for the AI Gateway module.
/// </summary>
public interface IAiGatewayModule
{
    /// <summary>
    /// Acquires a route lease reservation for a user and activity.
    /// </summary>
    Task<CreateRouteLeaseResult> CreateRouteLeaseAsync(
        CreateRouteLeaseRequest request,
        CancellationToken ct);

    /// <summary>
    /// Confirms AI model usage and records metadata (tokens, timings).
    /// </summary>
    Task<ConfirmRouteUsageResult> ConfirmRouteUsageAsync(
        ConfirmRouteUsageRequest request,
        CancellationToken ct);

    /// <summary>
    /// Releases a reservation lease if the call failed or was cancelled.
    /// </summary>
    Task<ReleaseRouteLeaseResult> ReleaseRouteLeaseAsync(
        ReleaseRouteLeaseRequest request,
        CancellationToken ct);

    /// <summary>
    /// Executes a secure chat completion utilizing an active lease.
    /// Insulates the caller from provider details, raw secrets, and credentials.
    /// </summary>
    Task<ExecuteChatCompletionResult> ExecuteChatCompletionAsync(
        ExecuteChatCompletionRequest request,
        CancellationToken ct);
}
