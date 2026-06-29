using EnglishTutor.AiGateway.Application.Abstractions.Audit;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Domain.Aggregates.AiVoice;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.AiGateway.Application.Admin.Voices.Commands.UpdateVoice;

internal sealed class UpdateVoiceCommandHandler : ICommandHandler<UpdateVoiceCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public UpdateVoiceCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(
        UpdateVoiceCommand command,
        CancellationToken cancellationToken)
    {
        var voice = await _unitOfWork.Voices.GetByIdAsync(
            command.VoiceId,
            cancellationToken);
        if (voice is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Voice"));
        }

        if (!Enum.TryParse<AiVoiceGender>(command.Gender, true, out var gender))
        {
            return Result.Failure(AiGatewayAdminErrors.Validation("Invalid voice gender."));
        }

        voice.Update(command.DisplayName, command.Style, gender, command.IsActive);
        _unitOfWork.Voices.Update(voice);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.RecordAsync(
            AiGatewayAuditActions.VoiceUpdated,
            "AiVoice",
            voice.Id.ToString(),
            new { voice.DisplayName, voice.Style, voice.Gender, voice.IsActive },
            command.ActorUserId,
            cancellationToken);

        return Result.Success();
    }
}
