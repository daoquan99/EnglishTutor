using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.UpdateProvider;

internal sealed class UpdateProviderCommandHandler : ICommandHandler<UpdateProviderCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public UpdateProviderCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(UpdateProviderCommand command, CancellationToken ct)
    {
        var provider = await _unitOfWork.Providers.GetByIdAsync(command.Id, ct);
        if (provider is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Provider"));
        }

        provider.Update(command.Name, command.IsActive);
        _unitOfWork.Providers.Update(provider);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.ProviderUpdated, "AiProvider", provider.Id.ToString(),
            new { provider.Name, provider.IsActive }, command.ActorUserId, ct);

        return Result.Success();
    }
}
