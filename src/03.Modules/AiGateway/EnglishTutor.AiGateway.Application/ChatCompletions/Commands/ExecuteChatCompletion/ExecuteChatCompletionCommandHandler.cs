using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;

namespace EnglishTutor.AiGateway.Application.ChatCompletions.Commands.ExecuteChatCompletion;

internal sealed class ExecuteChatCompletionCommandHandler : ICommandHandler<ExecuteChatCompletionCommand, ExecuteChatCompletionResult>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiProviderExecutionGateway _executionGateway;
    private readonly IDateTimeProvider _clock;

    public ExecuteChatCompletionCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiProviderExecutionGateway executionGateway,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork;
        _executionGateway = executionGateway;
        _clock = clock;
    }

    public async Task<Result<ExecuteChatCompletionResult>> Handle(ExecuteChatCompletionCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var lease = await _unitOfWork.RouteLeases.GetByIdAsync(request.LeaseId, ct);
        if (lease is null)
        {
            return Result.Success(new ExecuteChatCompletionResult { Status = ExecuteChatCompletionStatus.LeaseNotFound, ErrorCode = "aigateway.execute.not_found" });
        }

        if (lease.Status != AiRouteLeaseStatus.Reserved || lease.ExpiryAtUtc < _clock.UtcNow)
        {
            return Result.Success(new ExecuteChatCompletionResult { Status = ExecuteChatCompletionStatus.InvalidLeaseState, ErrorCode = "aigateway.execute.lease_invalid" });
        }

        var execution = await _executionGateway.ExecuteAsync(
            new ProviderExecutionRequest
            {
                LeaseId = lease.Id,
                SystemPrompt = request.SystemPrompt ?? string.Empty,
                UserPrompt = request.UserPrompt ?? string.Empty,
                CorrelationId = request.CorrelationId,
            },
            ct);

        if (!execution.IsSuccess)
        {
            return Result.Success(new ExecuteChatCompletionResult
            {
                Status = ExecuteChatCompletionStatus.ConfigurationMissing,
                ErrorCode = execution.ErrorCode ?? "aigateway.execute.provider_error",
            });
        }

        return Result.Success(new ExecuteChatCompletionResult
        {
            Status = ExecuteChatCompletionStatus.Success,
            ResponseText = execution.ResponseText,
            PromptTokens = execution.PromptTokens,
            CompletionTokens = execution.CompletionTokens,
            LatencyMs = execution.LatencyMs,
        });
    }
}
