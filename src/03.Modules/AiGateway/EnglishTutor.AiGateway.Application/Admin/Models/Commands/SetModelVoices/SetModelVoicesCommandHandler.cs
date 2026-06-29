using EnglishTutor.AiGateway.Application.Abstractions.Audit;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.SetModelVoices;

internal sealed class SetModelVoicesCommandHandler : ICommandHandler<SetModelVoicesCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public SetModelVoicesCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(
        SetModelVoicesCommand command,
        CancellationToken cancellationToken)
    {
        var model = await _unitOfWork.Models.GetByIdAsync(
            command.ModelId,
            cancellationToken);
        if (model is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Model"));
        }

        foreach (var voiceId in command.VoiceIds.Distinct())
        {
            var voice = await _unitOfWork.Voices.GetByIdAsync(voiceId, cancellationToken);
            if (voice is null || voice.ProviderId != model.ProviderId)
            {
                return Result.Failure(AiGatewayAdminErrors.Validation(
                    "Every voice must exist and belong to the model provider."));
            }
        }

        model.SetVoices(command.VoiceIds, command.DefaultVoiceId);
        _unitOfWork.Models.Update(model);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.RecordAsync(
            AiGatewayAuditActions.ModelVoicesChanged,
            "AiModel",
            model.Id.ToString(),
            new { VoiceCount = command.VoiceIds.Distinct().Count(), command.DefaultVoiceId },
            command.ActorUserId,
            cancellationToken);

        return Result.Success();
    }
}
