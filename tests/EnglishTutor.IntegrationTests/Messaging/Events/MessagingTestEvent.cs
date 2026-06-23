using EnglishTutor.BuildingBlocks.Contracts.Events;

namespace EnglishTutor.IntegrationTests.Messaging.Events;

/// <summary>
/// A test-only integration event used strictly for verifying the messaging pipeline.
/// </summary>
public sealed record MessagingTestEvent(string Value) : IntegrationEvent;
