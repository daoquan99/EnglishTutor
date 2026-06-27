using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.AiGateway.Application.Abstractions.Audit;
using EnglishTutor.Audit.Contracts;

namespace EnglishTutor.AiGateway.Infrastructure.Audit;

/// <summary>
/// Forwards AiGateway administrative audit events to the Audit module via its
/// public contract. Detail payloads carry only safe identifiers and state; no
/// raw or encrypted provider secret is ever serialized here.
/// </summary>
internal sealed class AuditModuleAiGatewayAuditPort : IAiGatewayAuditPort
{
    private readonly IAuditModule _auditModule;

    public AuditModuleAiGatewayAuditPort(IAuditModule auditModule)
    {
        _auditModule = auditModule;
    }

    public Task RecordAsync(
        string action,
        string entityType,
        string entityId,
        object? detail,
        Guid? actorUserId,
        CancellationToken ct)
    {
        var request = new RecordAuditLogRequest(
            UserId: actorUserId,
            Action: action,
            EntityType: entityType,
            EntityId: entityId,
            DetailJson: detail is null ? "{}" : JsonSerializer.Serialize(detail),
            IpAddressHash: null,
            UserAgentHash: null,
            CorrelationId: null,
            CreatedAtUtc: DateTime.UtcNow);

        return _auditModule.RecordAsync(request, ct);
    }
}
