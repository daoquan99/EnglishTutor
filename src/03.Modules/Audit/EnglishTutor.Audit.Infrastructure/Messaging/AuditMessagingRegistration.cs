using EnglishTutor.Audit.Infrastructure.Consumers;
using MassTransit;

namespace EnglishTutor.Audit.Infrastructure.Messaging;

public static class AuditMessagingRegistration
{
    public static void AddAuditSecurityEventConsumers(this IBusRegistrationConfigurator configurator)
    {
        configurator.AddConsumer<IdentitySecurityEventConsumer, IdentitySecurityEventConsumerDefinition>();
    }

    public static void AddAuditSecurityEventConsumers<TBus>(this IBusRegistrationConfigurator<TBus> configurator)
        where TBus : class, IBus
    {
        configurator.AddConsumer<IdentitySecurityEventConsumer, IdentitySecurityEventConsumerDefinition>();
    }
}
