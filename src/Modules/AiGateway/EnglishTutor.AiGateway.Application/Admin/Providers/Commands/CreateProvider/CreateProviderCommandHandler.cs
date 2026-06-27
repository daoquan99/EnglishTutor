using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProvider;

namespace EnglishTutor.AiGateway.Application.Admin.Providers.Commands.CreateProvider;

internal sealed class CreateProviderCommandHandler : ICommandHandler<CreateProviderCommand, Guid>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiGatewayAuditPort _audit;

    public CreateProviderCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public async Task<Result<Guid>> Handle(CreateProviderCommand command, CancellationToken ct)
    {
        if (await _unitOfWork.Providers.GetByCodeAsync(command.Code, ct) is not null)
        {
            return Result.Failure<Guid>(AiGatewayAdminErrors.Conflict("A provider with this code already exists."));
        }

        var provider = AiProvider.Create(Guid.NewGuid(), command.Name, command.Code, command.IsActive);
        await _unitOfWork.Providers.AddAsync(provider, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.ProviderCreated, "AiProvider", provider.Id.ToString(),
            new { provider.Name, provider.Code, provider.IsActive }, command.ActorUserId, ct);

        return Result.Success(provider.Id);
    }
}
