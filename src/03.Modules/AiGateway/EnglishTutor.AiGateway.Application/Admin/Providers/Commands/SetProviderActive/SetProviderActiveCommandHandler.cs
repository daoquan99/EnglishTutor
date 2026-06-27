using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.SetProviderActive;

internal sealed class SetProviderActiveCommandHandler : ICommandHandler<SetProviderActiveCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public SetProviderActiveCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(SetProviderActiveCommand command, CancellationToken ct)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(command.Id, ct);
        if (provider is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Provider"));
        }

        if (command.IsActive) provider.Activate(); else provider.Deactivate();
        _unitOfWork.Providers.Update(provider);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.ProviderActiveChanged, "AiProvider", provider.Id.ToString(),
            new { command.IsActive }, command.ActorUserId, ct);

        return Result.Success();
    }
}
