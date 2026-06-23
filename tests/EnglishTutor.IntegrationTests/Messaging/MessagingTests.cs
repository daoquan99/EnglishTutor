using EnglishTutor.IntegrationTests.Messaging.Consumers;
using EnglishTutor.IntegrationTests.Messaging.Events;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.RabbitMq;
using Xunit;

namespace EnglishTutor.IntegrationTests.Messaging;

/// <summary>
/// A custom Fact attribute that dynamically skips RabbitMQ tests unless the environment variable is explicitly set.
/// </summary>
public class RabbitMqTestFactAttribute : FactAttribute
{
    public RabbitMqTestFactAttribute()
    {
        var runTests = Environment.GetEnvironmentVariable("RUN_RABBITMQ_TESTS");
        if (runTests != "true")
        {
            Skip = "Skipped because RUN_RABBITMQ_TESTS is not set to true.";
        }
    }
}

public class MessagingTests
{
    [Fact]
    public async Task MassTransit_InMemoryHarness_ShouldConsumeTestEvent()
    {
        // Arrange
        await using var provider = new ServiceCollection()
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<MessagingTestConsumer>();
            })
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();

        await harness.Start();

        // Act
        await harness.Bus.Publish(new MessagingTestEvent("Hello World"));

        // Assert
        (await harness.Consumed.Any<MessagingTestEvent>()).Should().BeTrue();
        
        var consumed = harness.Consumed.Select<MessagingTestEvent>().First();
        consumed.Context.Message.Value.Should().Be("Hello World");
    }

    [RabbitMqTestFact]
    public async Task RabbitMq_Testcontainers_ShouldConsumeTestEvent_WhenEnabled()
    {
        // Start RabbitMQ container
        var rabbitMqContainer = new RabbitMqBuilder("rabbitmq:4-management-alpine")
            .Build();

        await rabbitMqContainer.StartAsync();

        try
        {
            var hostname = rabbitMqContainer.Hostname;
            var port = rabbitMqContainer.GetMappedPublicPort(5672);

            await using var provider = new ServiceCollection()
                .AddMassTransit(x =>
                {
                    x.AddConsumer<MessagingTestConsumer>();

                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(hostname, port, "/", h =>
                        {
                            h.Username("guest");
                            h.Password("guest");
                        });

                        cfg.ReceiveEndpoint("english.tests.test_queue", e =>
                        {
                            e.ConfigureConsumer<MessagingTestConsumer>(context);
                        });
                    });
                })
                .BuildServiceProvider(true);

            var busControl = provider.GetRequiredService<IBusControl>();
            await busControl.StartAsync();

            try
            {
                lock (MessagingTestConsumer.ConsumedEvents)
                {
                    MessagingTestConsumer.ConsumedEvents.Clear();
                }

                await busControl.Publish(new MessagingTestEvent("Hello RabbitMQ"));

                // Poll for consumption
                for (int i = 0; i < 50; i++)
                {
                    lock (MessagingTestConsumer.ConsumedEvents)
                    {
                        if (MessagingTestConsumer.ConsumedEvents.Count > 0)
                            break;
                    }
                    await Task.Delay(100);
                }

                lock (MessagingTestConsumer.ConsumedEvents)
                {
                    MessagingTestConsumer.ConsumedEvents.Should().ContainSingle();
                    MessagingTestConsumer.ConsumedEvents[0].Value.Should().Be("Hello RabbitMQ");
                }
            }
            finally
            {
                await busControl.StopAsync();
            }
        }
        finally
        {
            await rabbitMqContainer.StopAsync();
            await rabbitMqContainer.DisposeAsync();
        }
    }
}
