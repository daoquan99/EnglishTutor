using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Audit.Contracts;
using EnglishTutor.Audit.Application.Commands.RecordAuditLog;
using MediatR;

namespace EnglishTutor.Audit.Infrastructure;

internal sealed class AuditModule : IAuditModule
{
    private readonly ISender _sender;

    public AuditModule(ISender sender)
    {
        _sender = sender;
    }

    public async Task<AuditRecordResult> RecordAsync(
        RecordAuditLogRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RecordAuditLogCommand(
            UserId: request.UserId,
            Action: request.Action,
            EntityType: request.EntityType,
            EntityId: request.EntityId,
            DetailJson: request.DetailJson,
            IpAddressHash: request.IpAddressHash,
            UserAgentHash: request.UserAgentHash,
            CorrelationId: request.CorrelationId,
            CreatedAtUtc: request.CreatedAtUtc);

        var result = await _sender.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return new AuditRecordResult(IsSuccess: true);
        }

        return new AuditRecordResult(
            IsSuccess: false,
            ErrorCode: result.Error.Code,
            ErrorMessage: result.Error.Message);
    }
}
