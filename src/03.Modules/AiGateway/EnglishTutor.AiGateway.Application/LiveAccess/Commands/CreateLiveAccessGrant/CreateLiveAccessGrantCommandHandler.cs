using EnglishTutor.AiGateway.Application.Abstractions.Live;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Contracts.Dtos;
using EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.AiGateway.Application.LiveAccess.Commands.CreateLiveAccessGrant;

internal sealed class CreateLiveAccessGrantCommandHandler
    : ICommandHandler<CreateLiveAccessGrantCommand, CreateLiveAccessGrantResult>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiLiveAccessGateway _gateway;
    private readonly IDateTimeProvider _clock;

    public CreateLiveAccessGrantCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiLiveAccessGateway gateway,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork;
        _gateway = gateway;
        _clock = clock;
    }

    public async Task<Result<CreateLiveAccessGrantResult>> Handle(
        CreateLiveAccessGrantCommand command,
        CancellationToken cancellationToken)
    {
        var lease = await _unitOfWork.RouteLeases.GetByIdAsync(
            command.Request.LeaseId,
            cancellationToken);

        if (lease is null)
        {
            return Result.Success(Failure(
                CreateLiveAccessGrantStatus.LeaseNotFound,
                "aigateway.live.lease_not_found"));
        }

        if (lease.Status != AiRouteLeaseStatus.Reserved || lease.ExpiryAtUtc <= _clock.UtcNow)
        {
            return Result.Success(Failure(
                CreateLiveAccessGrantStatus.InvalidLeaseState,
                "aigateway.live.lease_invalid"));
        }

        var response = await _gateway.CreateAsync(
            new AiLiveAccessRequest(
                LeaseId: lease.Id,
                Capability: command.Request.Capability,
                VoiceId: command.Request.VoiceId,
                NativeLanguageCode: command.Request.NativeLanguageCode,
                TargetLanguageCode: command.Request.TargetLanguageCode),
            cancellationToken);

        return Result.Success(new CreateLiveAccessGrantResult
        {
            Status = response.IsSuccess
                ? CreateLiveAccessGrantStatus.Success
                : CreateLiveAccessGrantStatus.ProviderUnavailable,
            EphemeralToken = response.EphemeralToken,
            ProviderModelId = response.ProviderModelId,
            Capability = command.Request.Capability,
            VoiceId = response.VoiceId,
            ExpiresAtUtc = response.ExpiresAtUtc,
            ErrorCode = response.ErrorCode
        });
    }

    private static CreateLiveAccessGrantResult Failure(
        CreateLiveAccessGrantStatus status,
        string errorCode) =>
        new()
        {
            Status = status,
            ErrorCode = errorCode
        };
}
