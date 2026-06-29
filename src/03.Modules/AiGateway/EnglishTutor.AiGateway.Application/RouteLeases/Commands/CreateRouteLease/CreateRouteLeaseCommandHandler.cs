using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;

namespace EnglishTutor.AiGateway.Application.RouteLeases.Commands.CreateRouteLease;

internal sealed class CreateRouteLeaseCommandHandler : ICommandHandler<CreateRouteLeaseCommand, CreateRouteLeaseResult>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _clock;
    private readonly AiGatewayOptions _options;

    public CreateRouteLeaseCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IDateTimeProvider clock,
        IOptions<AiGatewayOptions> options)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
        _options = options.Value;
    }

    public async Task<Result<CreateRouteLeaseResult>> Handle(CreateRouteLeaseCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var existing = await _unitOfWork.RouteLeases.GetByIdempotencyKeyAsync(request.UserId, request.IdempotencyKey, ct);
        if (existing is not null)
        {
            var existingModel = await _unitOfWork.Models.GetByIdAsync(existing.ModelId, ct);
            var existingProvider = existingModel is null ? null : await _unitOfWork.Providers.GetByIdAsync(existingModel.ProviderId, ct);
            return Result.Success(new CreateRouteLeaseResult
            {
                LeaseId = existing.Id,
                ModelCode = existingModel?.Code,
                ProviderCode = existingProvider?.Code,
                Status = CreateRouteLeaseStatus.IdempotentRepeat,
                ExpiryAtUtc = existing.ExpiryAtUtc,
            });
        }

        var rule = await _unitOfWork.RoutingRules.FindActiveMatchAsync(
            request.ActivityType, request.TopicCode, request.ScenarioCode, ct);
        if (rule is null)
        {
            return Result.Success(new CreateRouteLeaseResult
            {
                Status = CreateRouteLeaseStatus.RouteNoMatch,
                ErrorCode = "aigateway.route_lease.no_match"
            });
        }

        var nowUtc = _clock.UtcNow;
        var requiredCapability = ParseCapability(request.RequiredCapability);
        if (requiredCapability is null)
        {
            return Result.Success(new CreateRouteLeaseResult
            {
                Status = CreateRouteLeaseStatus.ModelUnavailable,
                ErrorCode = "aigateway.route_lease.invalid_capability"
            });
        }

        var selection = await TrySelectModelAndKeyAsync(
            rule.PrimaryModelId,
            requiredCapability.Value,
            nowUtc,
            ct);
        if (selection is null && rule.FallbackModelId is Guid fallbackId)
        {
            selection = await TrySelectModelAndKeyAsync(
                fallbackId,
                requiredCapability.Value,
                nowUtc,
                ct);
        }

        if (selection is null)
        {
            return Result.Success(new CreateRouteLeaseResult
            {
                Status = CreateRouteLeaseStatus.ProviderKeyUnavailable,
                ErrorCode = "aigateway.route_lease.key_unavailable"
            });
        }

        var (model, key, providerCode) = selection.Value;
        var expiry = nowUtc.AddMinutes(_options.DefaultLeaseTtlMinutes);
        var lease = AiRouteLease.Create(
            Guid.NewGuid(), request.UserId, rule.Id, model.Id, key.Id, expiry, request.IdempotencyKey);

        await _unitOfWork.RouteLeases.AddAsync(lease, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new CreateRouteLeaseResult
        {
            LeaseId = lease.Id,
            ModelCode = model.Code,
            ProviderCode = providerCode,
            Status = CreateRouteLeaseStatus.Success,
            ExpiryAtUtc = lease.ExpiryAtUtc,
        });
    }

    private async Task<(AiModel Model, AiProviderKey Key, string ProviderCode)?> TrySelectModelAndKeyAsync(
        Guid modelId,
        AiModelCapability requiredCapability,
        DateTime nowUtc,
        CancellationToken ct)
    {
        var model = await _unitOfWork.Models.GetByIdAsync(modelId, ct);
        if (model is null ||
            !model.IsActive ||
            model.Lifecycle == AiModelLifecycle.Deprecated ||
            !model.Supports(requiredCapability))
        {
            return null;
        }

        var provider = await _unitOfWork.Providers.GetByIdAsync(model.ProviderId, ct);
        if (provider is null || !provider.IsActive)
        {
            return null;
        }

        var keys = await _unitOfWork.ProviderKeys.ListActiveByProviderByPriorityAsync(provider.Id, ct);
        var key = keys.FirstOrDefault(k => !k.IsOnCooldown(nowUtc));
        if (key is null)
        {
            return null;
        }

        return (model, key, provider.Code);
    }

    private static AiModelCapability? ParseCapability(string value)
    {
        var normalized = value.Replace("-", string.Empty, StringComparison.Ordinal);
        return Enum.TryParse<AiModelCapability>(normalized, true, out var capability)
            ? capability
            : null;
    }
}
