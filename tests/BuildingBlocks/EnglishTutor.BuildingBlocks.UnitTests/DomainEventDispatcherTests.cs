using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.BuildingBlocks.Application.DomainEvents;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.BuildingBlocks.Infrastructure.DomainEvents;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class DomainEventDispatcherTests
{
    public record TestDomainEvent(Guid Id) : IDomainEvent
    {
        public Guid EventId => Id;
        public DateTime OccurredAtUtc => DateTime.UtcNow;
    }

    [Fact]
    public async Task DispatchAsync_WithSuccessHandlers_ShouldCallAllInOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler1 = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        var handler2 = Substitute.For<IDomainEventHandler<TestDomainEvent>>();

        var callOrder = new List<string>();
        handler1.HandleAsync(Arg.Any<TestDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => { callOrder.Add("handler1"); return Task.CompletedTask; });
        handler2.HandleAsync(Arg.Any<TestDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => { callOrder.Add("handler2"); return Task.CompletedTask; });

        services.AddTransient<IDomainEventHandler<TestDomainEvent>>(sp => handler1);
        services.AddTransient<IDomainEventHandler<TestDomainEvent>>(sp => handler2);

        var provider = services.BuildServiceProvider();
        var dispatcher = new TransactionalDomainEventDispatcher(
            serviceProvider: provider,
            logger: NullLogger<TransactionalDomainEventDispatcher>.Instance);

        var domainEvent = new TestDomainEvent(Guid.NewGuid());

        // Act
        await dispatcher.DispatchAsync(
            events: new[] { domainEvent },
            cancellationToken: CancellationToken.None);

        // Assert
        callOrder.Should().Equal("handler1", "handler2");
    }

    [Fact]
    public async Task DispatchAsync_WhenFirstHandlerFails_ShouldPropagateExceptionAndNotRunSecondHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler1 = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        var handler2 = Substitute.For<IDomainEventHandler<TestDomainEvent>>();

        var runSecond = false;
        handler1.HandleAsync(Arg.Any<TestDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => throw new InvalidOperationException("First handler failed"));
        handler2.HandleAsync(Arg.Any<TestDomainEvent>(), Arg.Any<CancellationToken>())
            .Returns(x => { runSecond = true; return Task.CompletedTask; });

        services.AddTransient<IDomainEventHandler<TestDomainEvent>>(sp => handler1);
        services.AddTransient<IDomainEventHandler<TestDomainEvent>>(sp => handler2);

        var provider = services.BuildServiceProvider();
        var dispatcher = new TransactionalDomainEventDispatcher(
            serviceProvider: provider,
            logger: NullLogger<TransactionalDomainEventDispatcher>.Instance);

        var domainEvent = new TestDomainEvent(Guid.NewGuid());

        // Act
        var act = async () => await dispatcher.DispatchAsync(
            events: new[] { domainEvent },
            cancellationToken: CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("First handler failed");
        runSecond.Should().BeFalse();
    }

    [Fact]
    public async Task DispatchAsync_WhenCancelled_ShouldPropagateCancellationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var handler1 = Substitute.For<IDomainEventHandler<TestDomainEvent>>();
        services.AddTransient<IDomainEventHandler<TestDomainEvent>>(sp => handler1);

        var provider = services.BuildServiceProvider();
        var dispatcher = new TransactionalDomainEventDispatcher(
            serviceProvider: provider,
            logger: NullLogger<TransactionalDomainEventDispatcher>.Instance);

        var domainEvent = new TestDomainEvent(Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await dispatcher.DispatchAsync(
            events: new[] { domainEvent },
            cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DispatchAsync_WithZeroHandlers_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var dispatcher = new TransactionalDomainEventDispatcher(
            serviceProvider: provider,
            logger: NullLogger<TransactionalDomainEventDispatcher>.Instance);

        var domainEvent = new TestDomainEvent(Guid.NewGuid());

        // Act
        var act = async () => await dispatcher.DispatchAsync(
            events: new[] { domainEvent },
            cancellationToken: CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
