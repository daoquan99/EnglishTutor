namespace EnglishTutor.BuildingBlocks.Infrastructure.Messaging;

public sealed record MessageConsumerRegistration(
    Type HandlerType,
    MessageConsumerDescriptor Descriptor);
