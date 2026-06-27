using EnglishTutor.Audit.Infrastructure.Persistence;
using MassTransit;

namespace EnglishTutor.Audit.Infrastructure.Consumers;

/// <summary>
/// Binds the <see cref="IdentitySecurityEventConsumer"/> receive endpoint to the
/// Audit Entity Framework <b>inbox</b> (Batch R1, H-07) so consumption is
/// idempotent: the inbox dedup row and the recorded <c>SecurityEvent</c> row
/// commit together in one <c>AuditDbContext</c> transaction. Keeping this
/// binding in a definition keeps the Audit-specific inbox concern inside the
/// Audit module rather than in the shared host messaging composition.
/// </summary>
public sealed class IdentitySecurityEventConsumerDefinition
    : ConsumerDefinition<IdentitySecurityEventConsumer>
{
    public IdentitySecurityEventConsumerDefinition()
    {
        // Stable, descriptive endpoint name (transport queue / in-memory endpoint).
        EndpointName = "identity-security-events";
    }

    protected override void ConfigureConsumer(
        IReceiveEndpointConfigurator endpointConfigurator,
        IConsumerConfigurator<IdentitySecurityEventConsumer> consumerConfigurator,
        IRegistrationContext context)
    {
        endpointConfigurator.UseEntityFrameworkOutbox<AuditDbContext>(context);
    }
}
