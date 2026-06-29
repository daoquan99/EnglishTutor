using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.AiGateway.Application.RouteLeases.Commands.CreateRouteLease;
using EnglishTutor.AiGateway.Application.RouteLeases.Commands.ConfirmRouteUsage;
using EnglishTutor.AiGateway.Application.RouteLeases.Commands.ReleaseRouteLease;
using EnglishTutor.AiGateway.Application.ChatCompletions.Commands.ExecuteChatCompletion;
using EnglishTutor.AiGateway.Application.LiveAccess.Commands.CreateLiveAccessGrant;

namespace EnglishTutor.AiGateway.Application;

/// <summary>
/// A thin facade implementation of IAiGatewayModule that dispatches commands via MediatR.
/// </summary>
public sealed class AiGatewayService : IAiGatewayModule
{
    private readonly ISender _sender;

    public AiGatewayService(ISender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }

    public async Task<CreateRouteLeaseResult> CreateRouteLeaseAsync(CreateRouteLeaseRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new CreateRouteLeaseCommand(request), ct);
        if (!result.IsSuccess)
        {
            return new CreateRouteLeaseResult
            {
                Status = CreateRouteLeaseStatus.ValidationError,
                ErrorCode = "aigateway.route_lease.validation"
            };
        }
        return result.Value!;
    }

    public async Task<ConfirmRouteUsageResult> ConfirmRouteUsageAsync(ConfirmRouteUsageRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new ConfirmRouteUsageCommand(request), ct);
        if (!result.IsSuccess)
        {
            return new ConfirmRouteUsageResult
            {
                Status = ConfirmRouteUsageStatus.ValidationError,
                ErrorCode = "aigateway.confirm.validation"
            };
        }
        return result.Value!;
    }

    public async Task<ReleaseRouteLeaseResult> ReleaseRouteLeaseAsync(ReleaseRouteLeaseRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new ReleaseRouteLeaseCommand(request), ct);
        if (!result.IsSuccess)
        {
            return new ReleaseRouteLeaseResult
            {
                Status = ReleaseRouteLeaseStatus.ValidationError,
                ErrorCode = "aigateway.release.validation"
            };
        }
        return result.Value!;
    }

    public async Task<ExecuteChatCompletionResult> ExecuteChatCompletionAsync(ExecuteChatCompletionRequest request, CancellationToken ct)
    {
        var result = await _sender.Send(new ExecuteChatCompletionCommand(request), ct);
        if (!result.IsSuccess)
        {
            return new ExecuteChatCompletionResult
            {
                Status = ExecuteChatCompletionStatus.ValidationError,
                ErrorCode = "aigateway.execute.validation"
            };
        }
        return result.Value!;
    }

    public async Task<GenerateContentResult> GenerateContentAsync(GenerateContentRequest request, CancellationToken ct)
    {
        var result = await ExecuteChatCompletionAsync(
            new ExecuteChatCompletionRequest
            {
                LeaseId = request.LeaseId,
                SystemPrompt = request.SystemInstruction,
                UserPrompt = request.UserContent,
                ResponseJsonSchema = request.ResponseJsonSchema,
                CorrelationId = request.CorrelationId
            },
            ct);

        var structured = !string.IsNullOrWhiteSpace(request.ResponseJsonSchema);
        return new GenerateContentResult
        {
            Status = result.Status,
            Text = structured ? null : result.ResponseText,
            Json = structured ? result.ResponseText : null,
            SchemaCode = structured ? request.SchemaCode : null,
            SchemaVersion = structured ? request.SchemaVersion : null,
            PromptTokens = result.PromptTokens,
            CompletionTokens = result.CompletionTokens,
            LatencyMs = result.LatencyMs,
            ErrorCode = result.ErrorCode
        };
    }

    public async Task<CreateLiveAccessGrantResult> CreateLiveAccessGrantAsync(
        CreateLiveAccessGrantRequest request,
        CancellationToken ct)
    {
        var result = await _sender.Send(new CreateLiveAccessGrantCommand(request), ct);
        return result.IsSuccess
            ? result.Value!
            : new CreateLiveAccessGrantResult
            {
                Status = CreateLiveAccessGrantStatus.ValidationError,
                ErrorCode = "aigateway.live.validation"
            };
    }
}
