using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Application.Abstractions;

/// <summary>
/// Infrastructure-implemented execution gateway abstraction.
/// Performs key resolution, safe decryption, and external provider HTTP calls.
/// </summary>
public interface IAiProviderExecutionGateway
{
    Task<ProviderExecutionResponse> ExecuteAsync(
        ProviderExecutionRequest request,
        CancellationToken ct);
}
