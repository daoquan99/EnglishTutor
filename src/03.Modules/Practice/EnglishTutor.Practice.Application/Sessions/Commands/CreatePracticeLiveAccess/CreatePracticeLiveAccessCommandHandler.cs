using EnglishTutor.AiGateway.Contracts;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Practice.Application.Sessions.Policies;
using EnglishTutor.Practice.Contracts.Dtos;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.Repositories;
using EnglishTutor.Practice.Domain.Aggregates.PracticeSession.ValueObjects;

namespace EnglishTutor.Practice.Application.Sessions.Commands.CreatePracticeLiveAccess;

internal sealed class CreatePracticeLiveAccessCommandHandler
    : ICommandHandler<CreatePracticeLiveAccessCommand, PracticeLiveAccessResult>
{
    private readonly IPracticeSessionRepository _sessions;
    private readonly IAiGatewayModule _aiGateway;

    public CreatePracticeLiveAccessCommandHandler(
        IPracticeSessionRepository sessions,
        IAiGatewayModule aiGateway)
    {
        _sessions = sessions;
        _aiGateway = aiGateway;
    }

    public async Task<Result<PracticeLiveAccessResult>> Handle(
        CreatePracticeLiveAccessCommand command,
        CancellationToken cancellationToken)
    {
        var session = await _sessions.GetByIdAsync(command.SessionId, cancellationToken);
        if (session is null)
        {
            return Result.Success(Failure("NotFound", "practice.live.not_found"));
        }

        if (session.UserId != command.UserId)
        {
            return Result.Success(Failure("Forbidden", "practice.live.forbidden"));
        }

        if (session.Status != PracticeSessionStatus.Active)
        {
            return Result.Success(Failure("SessionNotActive", "practice.live.not_active"));
        }

        var capability = PracticeAiCapabilityPolicy.ForMode(session.ScenarioSnapshot.ModeCode);
        if (capability == PracticeAiCapabilityPolicy.ContentGeneration)
        {
            return Result.Success(Failure(
                "CapabilityMismatch",
                "practice.live.mode_not_supported"));
        }

        var grant = await _aiGateway.CreateLiveAccessGrantAsync(
            new CreateLiveAccessGrantRequest
            {
                LeaseId = session.RouteLeaseId,
                Capability = capability,
                VoiceId = command.VoiceId,
                NativeLanguageCode = command.NativeLanguageCode,
                TargetLanguageCode = command.TargetLanguageCode
            },
            cancellationToken);

        return Result.Success(new PracticeLiveAccessResult(
            Status: grant.Status.ToString(),
            EphemeralToken: grant.EphemeralToken,
            ProviderModelId: grant.ProviderModelId,
            Capability: grant.Capability,
            VoiceId: grant.VoiceId,
            ExpiresAtUtc: grant.ExpiresAtUtc,
            ErrorCode: grant.ErrorCode));
    }

    private static PracticeLiveAccessResult Failure(string status, string errorCode) =>
        new(status, null, null, null, null, null, errorCode);
}
