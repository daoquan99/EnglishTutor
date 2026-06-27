using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Audit.Application.Abstractions.Persistence;
using EnglishTutor.Audit.Domain.Aggregates.AuditLogs;
using EnglishTutor.Audit.Domain.Aggregates.AuditLogs.Repositories;

namespace EnglishTutor.Audit.Application.Commands.RecordAuditLog;

public sealed class RecordAuditLogCommandHandler : ICommandHandler<RecordAuditLogCommand>
{
    private readonly IAuditLogRepository _repository;
    private readonly IAuditUnitOfWork _unitOfWork;

    public RecordAuditLogCommandHandler(
        IAuditLogRepository repository,
        IAuditUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RecordAuditLogCommand request,
        CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.Create(
            userId: request.UserId,
            action: request.Action,
            entityType: request.EntityType,
            entityId: request.EntityId,
            detailJson: request.DetailJson,
            ipAddressHash: request.IpAddressHash,
            userAgentHash: request.UserAgentHash,
            correlationId: request.CorrelationId,
            createdAtUtc: request.CreatedAtUtc);

        await _repository.AddAsync(auditLog, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
