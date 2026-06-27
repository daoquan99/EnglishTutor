using System;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.AiGateway.Application.Abstractions.Audit;

/// <summary>
/// Application-owned audit port for AiGateway administrative changes. The
/// Infrastructure implementation forwards to the Audit module. Detail payloads
/// must contain only safe identifiers and state — never raw or encrypted
/// provider secrets.
/// </summary>
public interface IAiGatewayAuditPort
{
    Task RecordAsync(
        string action,
        string entityType,
        string entityId,
        object? detail,
        Guid? actorUserId,
        CancellationToken ct);
}

/// <summary>Stable AiGateway audit action codes.</summary>
public static class AiGatewayAuditActions
{
    public const string ProviderCreated = "aigateway.provider.created";
    public const string ProviderUpdated = "aigateway.provider.updated";
    public const string ProviderActiveChanged = "aigateway.provider.active_changed";

    public const string ModelCreated = "aigateway.model.created";
    public const string ModelUpdated = "aigateway.model.updated";
    public const string ModelActiveChanged = "aigateway.model.active_changed";

    public const string KeyCreated = "aigateway.key.created";
    public const string KeyDisabled = "aigateway.key.disabled";

    public const string RoutingRuleCreated = "aigateway.routing_rule.created";
    public const string RoutingRuleUpdated = "aigateway.routing_rule.updated";
    public const string RoutingRuleActiveChanged = "aigateway.routing_rule.active_changed";
}
