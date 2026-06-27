using System;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.AiGateway.Application.Abstractions.Persistence;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;
using EnglishTutor.AiGateway.Application.Abstractions.Security;
using EnglishTutor.AiGateway.Domain.Aggregates.AiProviderKey;

namespace EnglishTutor.AiGateway.Application.Admin.ProviderKeys.Commands.CreateProviderKey;

internal sealed class CreateProviderKeyCommandHandler : ICommandHandler<CreateProviderKeyCommand, Guid>
{
    private readonly IAiGatewayUnitOfWork _unitOfWork;
    private readonly IAiKeyProtector _keyProtector;
    private readonly IAiGatewayAuditPort _audit;

    public CreateProviderKeyCommandHandler(
        IAiGatewayUnitOfWork unitOfWork,
        IAiKeyProtector keyProtector,
        IAiGatewayAuditPort audit)
    {
        _unitOfWork = unitOfWork;
        _keyProtector = keyProtector;
        _audit = audit;
    }

    public async Task<Result<Guid>> Handle(CreateProviderKeyCommand command, CancellationToken ct)
    {
        if (await _unitOfWork.Providers.GetByIdAsync(command.ProviderId, ct) is null)
        {
            return Result.Failure<Guid>(AiGatewayAdminErrors.NotFound("Provider"));
        }

        var encrypted = _keyProtector.Encrypt(command.Secret);
        var mask = _keyProtector.Mask(command.Secret);
        var key = AiProviderKey.Create(Guid.NewGuid(), command.ProviderId, command.Name, encrypted, mask, command.Priority, command.IsActive);
        await _unitOfWork.ProviderKeys.AddAsync(key, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync(AiGatewayAuditActions.KeyCreated, "AiProviderKey", key.Id.ToString(),
            new { key.ProviderId, key.Name, key.KeyMask, key.Priority, key.IsActive }, command.ActorUserId, ct);

        return Result.Success(key.Id);
    }
}
