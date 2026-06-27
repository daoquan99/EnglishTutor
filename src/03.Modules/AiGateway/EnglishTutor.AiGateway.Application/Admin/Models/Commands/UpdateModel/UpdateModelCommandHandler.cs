using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.UpdateModel;

internal sealed class UpdateModelCommandHandler : ICommandHandler<UpdateModelCommand>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public UpdateModelCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result> Handle(UpdateModelCommand command, CancellationToken ct)
    {
        var model = await _unitOfWork.Models.GetByIdAsync(command.Id, ct);
        if (model is null)
        {
            return Result.Failure(AiGatewayAdminErrors.NotFound("Model"));
        }

        model.Update(command.Name, command.Capabilities ?? [], command.IsActive);
        _unitOfWork.Models.Update(model);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.ModelUpdated, "AiModel", model.Id.ToString(),
            new { model.Name, model.IsActive }, command.ActorUserId, ct);

        return Result.Success();
    }
}
