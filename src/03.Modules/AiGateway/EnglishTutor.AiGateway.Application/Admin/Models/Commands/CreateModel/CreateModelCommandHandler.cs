using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;
using EnglishTutor.AiGateway.Domain.Aggregates.AiModel;

namespace EnglishTutor.AiGateway.Application.Admin.Models.Commands.CreateModel;

internal sealed class CreateModelCommandHandler : ICommandHandler<CreateModelCommand, Guid>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public CreateModelCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result<Guid>> Handle(CreateModelCommand command, CancellationToken ct)
    {
        if (await _unitOfWork.Providers.GetByIdAsync(command.ProviderId, ct) is null)
        {
            return Result.Failure<Guid>(AiGatewayAdminErrors.NotFound("Provider"));
        }

        if (await _unitOfWork.Models.GetByCodeAsync(command.Code, ct) is not null)
        {
            return Result.Failure<Guid>(AiGatewayAdminErrors.Conflict("A model with this code already exists."));
        }

        var model = AiModel.Create(Guid.NewGuid(), command.ProviderId, command.Name, command.Code, command.Capabilities ?? [], command.IsActive);
        await _unitOfWork.Models.AddAsync(model, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.ModelCreated, "AiModel", model.Id.ToString(),
            new { model.ProviderId, model.Name, model.Code, model.IsActive }, command.ActorUserId, ct);

        return Result.Success(model.Id);
    }
}
