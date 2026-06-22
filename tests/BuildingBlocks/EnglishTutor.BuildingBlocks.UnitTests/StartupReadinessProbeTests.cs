using EnglishTutor.BuildingBlocks.Infrastructure.HealthChecks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnglishTutor.BuildingBlocks.UnitTests;

public class StartupReadinessProbeTests
{
    [Fact]
    public async Task StartAsync_Should_Stop_Application_When_No_Checks_Registered_Healthy()
    {
        // Arrange: register a check that always returns Unhealthy.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks()
            .AddCheck("always-unhealthy", () => HealthCheckResult.Unhealthy("simulated failure"), tags: new[] { "ready" });
        var sp = services.BuildServiceProvider();

        var lifetime = new ApplicationLifetime();
        var sut = new StartupReadinessProbe(
            sp, lifetime, NullLogger<StartupReadinessProbe>.Instance);

        // Act
        await sut.StartAsync(CancellationToken.None);

        // Assert: host was asked to stop (graceful, no throw).
        lifetime.StopApplicationCalled.Should().BeTrue();
    }

    [Fact]
    public async Task StartAsync_Should_Not_Stop_When_All_Checks_Healthy()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHealthChecks()
            .AddCheck("healthy", () => HealthCheckResult.Healthy("ok"), tags: new[] { "ready" });
        var sp = services.BuildServiceProvider();

        var lifetime = new ApplicationLifetime();
        var sut = new StartupReadinessProbe(
            sp, lifetime, NullLogger<StartupReadinessProbe>.Instance);

        await sut.StartAsync(CancellationToken.None);

        lifetime.StopApplicationCalled.Should().BeFalse();
    }

    [Fact]
    public void Default_Timeout_Should_Be_60_Seconds()
    {
        // Regression: bumped from 30s to 60s for managed-DB cold-start handshake.
        StartupReadinessProbe.DefaultTimeout.Should().Be(TimeSpan.FromSeconds(60));
    }

    /// <summary>Test double for <see cref="IHostApplicationLifetime"/>.</summary>
    private sealed class ApplicationLifetime : IHostApplicationLifetime
    {
        public bool StopApplicationCalled { get; private set; }
        public CancellationToken ApplicationStarted => CancellationToken.None;
        public CancellationToken ApplicationStopping => CancellationToken.None;
        public CancellationToken ApplicationStopped => CancellationToken.None;
        public void StopApplication() => StopApplicationCalled = true;
    }
}
