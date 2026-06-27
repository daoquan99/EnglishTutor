using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.SetModelActive;

internal sealed class SetModelActiveCommandHandler : ICommandHandler<SetModelActiveCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public SetModelActiveCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(SetModelActiveCommand command, CancellationToken ct)
    {
        var model = await _unitOfWork.Models.GetByIdAsync(command.Id, ct);
        if (model is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Model"));
        }

        if (command.IsActive) model.Activate(); else model.Deactivate();
        _unitOfWork.Models.Update(model);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.ModelActiveChanged, "AiModel", model.Id.ToString(),
            new { command.IsActive }, command.ActorUserId, ct);

        return Result.Success();
    }
}
