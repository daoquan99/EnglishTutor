extern alias WorkerAssembly;
using EnglishTutor.Identity.Infrastructure.Persistence;
using EnglishTutor.Audit.Infrastructure.Persistence;
using EnglishTutor.Learning.Infrastructure.Persistence;
using WorkerOptions = WorkerAssembly::EnglishTutor.Worker.Options.WorkerOptions;
using WorkerSchemaReadinessHostedService = WorkerAssembly::EnglishTutor.Worker.Readiness.WorkerSchemaReadinessHostedService;
using ExpiredRefreshTokensCleanupHostedService = WorkerAssembly::EnglishTutor.Worker.Jobs.ExpiredRefreshTokensCleanupHostedService;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using MediatR;

namespace EnglishTutor.IntegrationTests;

public class WorkerTests
{
    [Fact]
    public void WorkerOptions_Validation_Should_Throw_On_Invalid_Values()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Worker:JobsEnabled"] = "true",
                ["Worker:RefreshTokenCleanupInterval"] = "00:00:00", // Invalid: must be > TimeSpan.Zero
                ["Worker:RefreshTokenRetentionDays"] = "0", // Invalid: must be >= 1
                ["Worker:ShutdownTimeout"] = "-00:00:10" // Invalid: must be > TimeSpan.Zero
            })
            .Build();

        services.AddOptions<WorkerOptions>()
            .Bind(configuration.GetSection(WorkerOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(o => o.RefreshTokenCleanupInterval > TimeSpan.Zero, 
                "Worker:RefreshTokenCleanupInterval must be greater than zero.")
            .Validate(o => o.RefreshTokenRetentionDays >= 1 && o.RefreshTokenRetentionDays <= 365, 
                "Worker:RefreshTokenRetentionDays must be between 1 and 365 days.")
            .Validate(o => o.ShutdownTimeout > TimeSpan.Zero,
                "Worker:ShutdownTimeout must be greater than zero.")
            .ValidateOnStart();

        var provider = services.BuildServiceProvider();

        // Act
        Action resolveOptions = () => _ = provider.GetRequiredService<IOptions<WorkerOptions>>().Value;

        // Assert
        resolveOptions.Should().Throw<OptionsValidationException>()
            .And.Message.Should().ContainAll(
                "Worker:RefreshTokenCleanupInterval must be greater than zero.",
                "Worker:RefreshTokenRetentionDays must be between 1 and 365 days.",
                "Worker:ShutdownTimeout must be greater than zero.");
    }

    [Fact]
    public void WorkerOptions_Validation_Should_Pass_On_Valid_Values()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Worker:JobsEnabled"] = "true",
                ["Worker:RefreshTokenCleanupInterval"] = "12:00:00",
                ["Worker:RefreshTokenRetentionDays"] = "30",
                ["Worker:ShutdownTimeout"] = "00:00:30"
            })
            .Build();

        services.AddOptions<WorkerOptions>()
            .Bind(configuration.GetSection(WorkerOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(o => o.RefreshTokenCleanupInterval > TimeSpan.Zero, 
                "Worker:RefreshTokenCleanupInterval must be greater than zero.")
            .Validate(o => o.RefreshTokenRetentionDays >= 1 && o.RefreshTokenRetentionDays <= 365, 
                "Worker:RefreshTokenRetentionDays must be between 1 and 365 days.")
            .Validate(o => o.ShutdownTimeout > TimeSpan.Zero,
                "Worker:ShutdownTimeout must be greater than zero.")
            .ValidateOnStart();

        var provider = services.BuildServiceProvider();

        // Act
        var options = provider.GetRequiredService<IOptions<WorkerOptions>>().Value;

        // Assert
        options.JobsEnabled.Should().BeTrue();
        options.RefreshTokenCleanupInterval.Should().Be(TimeSpan.FromHours(12));
        options.RefreshTokenRetentionDays.Should().Be(30);
        options.ShutdownTimeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task WorkerSchemaReadiness_Should_Pass_Startup_If_No_Pending_Migrations()
    {
        // Arrange
        var services = new ServiceCollection();

        var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase("IdentityTestDb_Schema")
            .Options;
        var auditOptions = new DbContextOptionsBuilder<AuditDbContext>()
            .UseInMemoryDatabase("AuditTestDb_Schema")
            .Options;
        var learningOptions = new DbContextOptionsBuilder<LearningDbContext>()
            .UseInMemoryDatabase("LearningTestDb_Schema")
            .Options;

        var identityDb = new IdentityDbContext(identityOptions);
        var auditDb = new AuditDbContext(auditOptions);
        var learningDb = new LearningDbContext(learningOptions);

        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        var scopedProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IServiceScopeFactory))
            .Returns(new FakeServiceScopeFactory(serviceScope));
        serviceScope.ServiceProvider.Returns(scopedProvider);
        scopedProvider.GetService(typeof(IdentityDbContext)).Returns(identityDb);
        scopedProvider.GetService(typeof(AuditDbContext)).Returns(auditDb);
        scopedProvider.GetService(typeof(LearningDbContext)).Returns(learningDb);

        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var workerOptions = new WorkerOptions { RequireSchemaMatchOnStartup = true };
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(workerOptions);

        var readinessService = new WorkerSchemaReadinessHostedService(
            serviceProvider,
            lifetime,
            NullLogger<WorkerSchemaReadinessHostedService>.Instance,
            optionsWrapper);

        // Act
        await readinessService.StartAsync(CancellationToken.None);

        // Assert
        // Since InMemory DB returns empty pending migrations, startup should NOT abort.
        lifetime.DidNotReceive().StopApplication();
        Environment.ExitCode.Should().NotBe(1);
    }

    [Fact]
    public async Task WorkerSchemaReadiness_Should_Abort_Startup_On_Database_Exception()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var serviceScope = Substitute.For<IServiceScope>();
        var scopedProvider = Substitute.For<IServiceProvider>();

        serviceProvider.GetService(typeof(IServiceScopeFactory))
            .Returns(new FakeServiceScopeFactory(serviceScope));
        serviceScope.ServiceProvider.Returns(scopedProvider);

        // Simulate database query exception by causing GetRequiredService to throw
        scopedProvider.GetService(typeof(IdentityDbContext))
            .Returns(x => throw new Exception("Simulated DB exception"));

        var lifetime = Substitute.For<IHostApplicationLifetime>();
        var workerOptions = new WorkerOptions { RequireSchemaMatchOnStartup = true };
        var optionsWrapper = Microsoft.Extensions.Options.Options.Create(workerOptions);

        var readinessService = new WorkerSchemaReadinessHostedService(
            serviceProvider,
            lifetime,
            NullLogger<WorkerSchemaReadinessHostedService>.Instance,
            optionsWrapper);

        // Set exit code to 0 before execution
        Environment.ExitCode = 0;

        try
        {
            // Act
            await readinessService.StartAsync(CancellationToken.None);

            // Assert
            lifetime.Received(1).StopApplication();
            Environment.ExitCode.Should().Be(1);
        }
        finally
        {
            // Clean up: reset ExitCode to 0 so it doesn't affect other tests/test runners.
            Environment.ExitCode = 0;
        }
    }
}

internal class FakeServiceScopeFactory : IServiceScopeFactory
{
    private readonly IServiceScope _scope;

    public FakeServiceScopeFactory(IServiceScope scope)
    {
        _scope = scope;
    }

    public IServiceScope CreateScope() => _scope;
}
