using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Application.Abstractions.Persistence;
using EnglishTutor.Practice.Application.Sessions.Abstractions;
using EnglishTutor.Practice.Application.Sessions.Policies;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;
using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;

namespace EnglishTutor.Practice.Application.Sessions.Commands.AppendPracticeMessage;

public sealed class AppendPracticeMessageHandler : ICommandHandler<AppendPracticeMessageCommand, AppendTranscriptResult>
{
    private const string UserRole = "user";
    private const string AssistantRole = "assistant";

    private readonly IPracticeSessionRepository _sessions;
    private readonly IPracticeUnitOfWork _unitOfWork;
    private readonly IAiGatewayModule _aiGateway;
    private readonly IDateTimeProvider _clock;
    private readonly IPracticeSessionResourceFinalizer _finalizer;

    public AppendPracticeMessageHandler(
        IPracticeSessionRepository sessions,
        IPracticeUnitOfWork unitOfWork,
        IAiGatewayModule aiGateway,
        IDateTimeProvider clock,
        IPracticeSessionResourceFinalizer finalizer)
    {
        _sessions = sessions;
        _unitOfWork = unitOfWork;
        _aiGateway = aiGateway;
        _clock = clock;
        _finalizer = finalizer;
    }

    public async Task<Result<AppendTranscriptResult>> Handle(AppendPracticeMessageCommand request, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(request.SessionId, ct);
        if (session is null)
        {
            return Result.Success(new AppendTranscriptResult(AppendTranscriptStatus.SessionNotFound, null, null, null, null, 0, 0, "practice.append.not_found"));
        }

        if (session.UserId != request.UserId)
        {
            return Result.Success(new AppendTranscriptResult(AppendTranscriptStatus.Forbidden, null, null, null, null, 0, 0, "practice.append.forbidden"));
        }

        var now = _clock.UtcNow;
        if (PracticeSessionExpiryPolicy.IsExpired(session, now))
        {
            session.Expire(now);
            await _finalizer.FinalizeResourcesAsync(session.QuotaReservationId, session.RouteLeaseId, isExpired: true, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success(new AppendTranscriptResult(AppendTranscriptStatus.SessionNotActive, null, null, null, null, 0, 0, "practice.append.not_active"));
        }

        if (session.Status != PracticeSessionStatus.Active)
        {
            return Result.Success(new AppendTranscriptResult(AppendTranscriptStatus.SessionNotActive, null, null, null, null, 0, 0, "practice.append.not_active"));
        }

        var userMessageId = Guid.NewGuid();
        session.AppendMessage(userMessageId, UserRole, request.Content, now);

        var execution = await _aiGateway.ExecuteChatCompletionAsync(new ExecuteChatCompletionRequest
        {
            LeaseId = session.RouteLeaseId,
            SystemPrompt = session.ScenarioSnapshot.LearnerFacingInstructions,
            UserPrompt = request.Content,
        }, ct);

        if (execution.Status != ExecuteChatCompletionStatus.Success || string.IsNullOrEmpty(execution.ResponseText))
        {
            await _unitOfWork.SaveChangesAsync(ct);
            return Result.Success(new AppendTranscriptResult(AppendTranscriptStatus.AiExecutionFailed, userMessageId, null, null, null, execution.PromptTokens, execution.CompletionTokens, execution.ErrorCode ?? "practice.append.ai_failed"));
        }

        var assistantMessageId = Guid.NewGuid();
        session.AppendMessage(assistantMessageId, AssistantRole, execution.ResponseText, now);

        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(new AppendTranscriptResult(
            AppendTranscriptStatus.Success, userMessageId, assistantMessageId, execution.ResponseText,
            null, execution.PromptTokens, execution.CompletionTokens, null));
    }
}
