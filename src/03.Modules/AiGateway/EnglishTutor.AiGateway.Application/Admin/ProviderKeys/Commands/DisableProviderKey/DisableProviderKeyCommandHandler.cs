using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.DisableProviderKey;

internal sealed class DisableProviderKeyCommandHandler : ICommandHandler<DisableProviderKeyCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public DisableProviderKeyCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(DisableProviderKeyCommand command, CancellationToken ct)
    {
        var key = await _unitOfWork.ProviderKeys.GetByIdAsync(command.Id, ct);
        if (key is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Provider key"));
        }

        key.Deactivate();
        _unitOfWork.ProviderKeys.Update(key);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.KeyDisabled, "AiProviderKey", key.Id.ToString(),
            new { key.ProviderId, key.KeyMask }, command.ActorUserId, ct);

        return Result.Success();
    }
}
