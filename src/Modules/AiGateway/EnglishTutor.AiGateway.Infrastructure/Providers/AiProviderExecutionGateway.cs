using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Application.Abstractions;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Providers;
using EnglishTutor.AiGateway.Application.Abstractions.Security;
using Microsoft.Extensions.Logging;

namespace EnglishTutor.AiGateway.Infrastructure.Providers;

/// <summary>
/// Resolves a reserved lease into a concrete provider call: loads the model and
/// provider, decrypts the chosen provider key at the last responsible moment,
/// dispatches to the matching <see cref="IAiProviderAdapter"/>, and returns only
/// safe execution metadata. The decrypted secret never leaves this method and is
/// never logged.
/// </summary>
internal sealed class AiProviderExecutionGateway : IAiProviderExecutionGateway
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiKeyProtector _keyProtector;
    private readonly IAiProviderAdapterRegistry _adapters;
    private readonly ILogger<AiProviderExecutionGateway> _logger;

    public AiProviderExecutionGateway(
        IAiGatewayUnitOfWork unitOfWork,
        IAiKeyProtector keyProtector,
        IAiProviderAdapterRegistry adapters,
        ILogger<AiProviderExecutionGateway> logger)
    {
        _unitOfWork = unitOfWork;
        _keyProtector = keyProtector;
        _adapters = adapters;
        _logger = logger;
    }

    public async Task<ProviderExecutionResponse> ExecuteAsync(
        ProviderExecutionRequest request,
        CancellationToken ct)
    {
        var lease = await _unitOfWork.RouteLeases.GetByIdAsync(request.LeaseId, ct);
        if (lease is null)
        {
            return Failure("lease.not_found");
        }

        var model = await _unitOfWork.Models.GetByIdAsync(lease.ModelId, ct);
        var key = await _unitOfWork.ProviderKeys.GetByIdAsync(lease.ProviderKeyId, ct);
        if (model is null || key is null)
        {
            return Failure("route.configuration_missing");
        }

        var provider = await _unitOfWork.Providers.GetByIdAsync(model.ProviderId, ct);
        if (provider is null)
        {
            return Failure("route.configuration_missing");
        }

        var adapter = _adapters.Resolve(provider.Code);
        if (adapter is null)
        {
            _logger.LogWarning("No AI provider adapter registered for provider code {ProviderCode}.", provider.Code);
            return Failure("provider.adapter_unavailable");
        }

        var plaintextKey = _keyProtector.Decrypt(key.EncryptedKey);
        var stopwatch = Stopwatch.StartNew();
        var adapterResponse = await adapter.ExecuteChatAsync(
            new AiProviderAdapterRequest(model.Code, plaintextKey, request.SystemPrompt, request.UserPrompt),
            ct);
        stopwatch.Stop();

        return new ProviderExecutionResponse
        {
            IsSuccess = adapterResponse.IsSuccess,
            ResponseText = adapterResponse.ResponseText,
            PromptTokens = adapterResponse.PromptTokens,
            CompletionTokens = adapterResponse.CompletionTokens,
            LatencyMs = adapterResponse.LatencyMs > 0 ? adapterResponse.LatencyMs : stopwatch.ElapsedMilliseconds,
            ErrorCode = adapterResponse.ErrorCode,
        };
    }

    private static ProviderExecutionResponse Failure(string errorCode) => new()
    {
        IsSuccess = false,
        ErrorCode = errorCode,
    };
}
