extern alias WorkerAssembly;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Application;
using EnglishTutor.Quota.Application.Abstractions.Persistence;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState.Repositories;
using QuotaExpiredReservationCleanupHostedService = WorkerAssembly::EnglishTutor.Worker.HostedServices.QuotaExpiredReservationCleanupHostedService;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

using MediatR;
using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.Quota.Application.Reservations.Commands.ExpireQuotaReservations;

namespace EnglishTutor.IntegrationTests.Quota;

public class QuotaCleanupWorkerTests
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IServiceScope _scope;
    private readonly IServiceProvider _serviceProvider;
    private readonly IQuotaUnitOfWork _unitOfWork;
    private readonly IQuotaReservationRepository _reservationRepository;
    private readonly IUserQuotaStateRepository _stateRepository;
    private readonly ILogger<QuotaExpiredReservationCleanupHostedService> _logger;
    private readonly QuotaOptions _options;
    private readonly QuotaExpiredReservationCleanupHostedService _worker;

    public QuotaCleanupWorkerTests()
    {
        _scopeFactory = Substitute.For<IServiceScopeFactory>();
        _scope = Substitute.For<IServiceScope>();
        _serviceProvider = Substitute.For<IServiceProvider>();
        _unitOfWork = Substitute.For<IQuotaUnitOfWork>();
        _reservationRepository = Substitute.For<IQuotaReservationRepository>();
        _stateRepository = Substitute.For<IUserQuotaStateRepository>();
        _logger = Substitute.For<ILogger<QuotaExpiredReservationCleanupHostedService>>();

        _scopeFactory.CreateScope().Returns(_scope);
        _scope.ServiceProvider.Returns(_serviceProvider);
        _serviceProvider.GetService(typeof(IQuotaUnitOfWork)).Returns(_unitOfWork);
        _serviceProvider.GetRequiredService<IQuotaUnitOfWork>().Returns(_unitOfWork);

        _unitOfWork.QuotaReservations.Returns(_reservationRepository);
        _unitOfWork.UserQuotaStates.Returns(_stateRepository);

        _options = new QuotaOptions
        {
            ExpiredReservationCleanupIntervalSeconds = 1,
            ExpiredReservationCleanupBatchSize = 5,
            MaxConcurrencyRetryCount = 3
        };

        var optionsWrapper = Options.Create(_options);
        var sender = Substitute.For<ISender>();
        var clock = Substitute.For<IDateTimeProvider>();
        clock.UtcNow.Returns(DateTime.UtcNow);
        var handlerLogger = Substitute.For<ILogger<ExpireQuotaReservationsCommandHandler>>();
        var handler = new ExpireQuotaReservationsCommandHandler(_unitOfWork, optionsWrapper, clock, handlerLogger);

        sender.Send(Arg.Any<ExpireQuotaReservationsCommand>(), Arg.Any<CancellationToken>())
            .Returns(async call => await handler.Handle((ExpireQuotaReservationsCommand)call[0], (CancellationToken)call[1]));

        _serviceProvider.GetService(typeof(ISender)).Returns(sender);
        _serviceProvider.GetRequiredService<ISender>().Returns(sender);

        var workerOptions = new WorkerAssembly::EnglishTutor.Worker.Options.WorkerOptions
        {
            JobsEnabled = true,
            EnableQuotaReservationExpiry = true,
            QuotaReservationExpiryInterval = TimeSpan.FromSeconds(1),
            QuotaReservationExpiryBatchSize = 5
        };
        var workerOptionsWrapper = Options.Create(workerOptions);
        _worker = new QuotaExpiredReservationCleanupHostedService(_scopeFactory, _logger, workerOptionsWrapper);
    }

    [Fact]
    public async Task ProcessExpiredReservationsAsync_WhenExpiredFound_ShouldMarkExpiredAndReleaseCounters()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var reservation = QuotaReservation.Create(
            Guid.NewGuid(), userId, "key-expire-1", 15, DateTime.UtcNow.AddMinutes(-1), DateTime.UtcNow.Date);
        var expiredList = new List<QuotaReservation> { reservation };

        _reservationRepository.GetExpiredReservationsAsync(Arg.Any<DateTime>(), _options.ExpiredReservationCleanupBatchSize, Arg.Any<CancellationToken>())
            .Returns(expiredList, new List<QuotaReservation>()); // return list first, then empty list to break loop

        _reservationRepository.GetByIdAsync(reservation.Id, Arg.Any<CancellationToken>()).Returns(reservation);

        var state = UserQuotaState.Create(Guid.NewGuid(), userId, DateTime.UtcNow.Date, 15, 0, 1, 0, 0);
        _stateRepository.GetByUserIdAndQuotaDateAsync(userId, DateTime.UtcNow.Date, Arg.Any<CancellationToken>()).Returns(state);
        _stateRepository.GetByIdAsync(state.Id, Arg.Any<CancellationToken>()).Returns(state);

        // Act
        // Invoke internal method via reflection or just call the public/protected interface if needed
        var method = typeof(QuotaExpiredReservationCleanupHostedService)
            .GetMethod("ProcessExpiredReservationsAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        await (Task)method!.Invoke(_worker, new object[] { CancellationToken.None })!;

        // Assert
        reservation.Status.Should().Be(QuotaReservation.ReservationStatus.Expired);
        state.ReservedMinutes.Should().Be(0);
        state.ReservedSessionCount.Should().Be(0);

        _reservationRepository.Received(1).Update(reservation);
        _stateRepository.Received(1).Update(state);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
