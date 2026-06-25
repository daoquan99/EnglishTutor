using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EnglishTutor.Quota.Application;
using EnglishTutor.Quota.Application.Abstractions.Persistence;
using EnglishTutor.Quota.Contracts;
using EnglishTutor.Quota.Contracts.Dtos;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule.Repositories;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace EnglishTutor.IntegrationTests.Quota;

public class QuotaServiceTests
{
    private readonly IQuotaUnitOfWork _unitOfWork;
    private readonly IUserQuotaRuleRepository _ruleRepository;
    private readonly IUserQuotaStateRepository _stateRepository;
    private readonly IQuotaReservationRepository _reservationRepository;
    private readonly IUsageLogRepository _usageLogRepository;
    private readonly IRateLimitEventRepository _rateLimitEventRepository;
    private readonly QuotaService _quotaService;
    private readonly QuotaOptions _options;

    public QuotaServiceTests()
    {
        _unitOfWork = Substitute.For<IQuotaUnitOfWork>();
        _ruleRepository = Substitute.For<IUserQuotaRuleRepository>();
        _stateRepository = Substitute.For<IUserQuotaStateRepository>();
        _reservationRepository = Substitute.For<IQuotaReservationRepository>();
        _usageLogRepository = Substitute.For<IUsageLogRepository>();
        _rateLimitEventRepository = Substitute.For<IRateLimitEventRepository>();

        _unitOfWork.UserQuotaRules.Returns(_ruleRepository);
        _unitOfWork.UserQuotaStates.Returns(_stateRepository);
        _unitOfWork.QuotaReservations.Returns(_reservationRepository);
        _unitOfWork.UsageLogs.Returns(_usageLogRepository);
        _unitOfWork.RateLimitEvents.Returns(_rateLimitEventRepository);

        _options = new QuotaOptions
        {
            DefaultDailyMaxSessionMinutes = 60,
            DefaultDailyMaxSessions = 3,
            DefaultMaxSingleSessionMinutes = 30,
            ReservationTtlMinutes = 15,
            MaxConcurrencyRetryCount = 3,
            ExpiredReservationCleanupIntervalSeconds = 60,
            ExpiredReservationCleanupBatchSize = 10
        };

        var optionsWrapper = Options.Create(_options);
        _quotaService = new QuotaService(_unitOfWork, optionsWrapper);
    }

    [Fact]
    public async Task ReserveSessionQuotaAsync_WhenUnderLimits_ShouldCreateReservationAndReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ReserveSessionQuotaRequest
        {
            UserId = userId,
            RequestedMinutes = 20,
            IdempotencyKey = "idempotency-key-1"
        };

        var rule = UserQuotaRule.Create(Guid.NewGuid(), userId, 60, 3, 30, DateTime.UtcNow.Date, true);
        _ruleRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(rule);

        var state = UserQuotaState.Create(Guid.NewGuid(), userId, DateTime.UtcNow.Date, 0, 0, 0, 0, 0);
        _stateRepository.GetByUserIdAndQuotaDateAsync(userId, DateTime.UtcNow.Date, Arg.Any<CancellationToken>()).Returns(state);
        _stateRepository.GetByIdAsync(state.Id, Arg.Any<CancellationToken>()).Returns(state);

        _reservationRepository.GetByIdempotencyKeyAsync(userId, request.IdempotencyKey, Arg.Any<CancellationToken>()).Returns((QuotaReservation?)null);

        // Act
        var result = await _quotaService.ReserveSessionQuotaAsync(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ReserveSessionQuotaStatus.Success);
        result.ReservationId.Should().NotBeNull();
        result.RemainingMinutes.Should().Be(40);
        result.RemainingSessions.Should().Be(2);

        state.ReservedMinutes.Should().Be(20);
        state.ReservedSessionCount.Should().Be(1);

        await _reservationRepository.Received(1).AddAsync(Arg.Any<QuotaReservation>(), Arg.Any<CancellationToken>());
        _stateRepository.Received(1).Update(state);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReserveSessionQuotaAsync_WhenExceedingDailyMinutes_ShouldReturnDailyMinutesExceeded()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ReserveSessionQuotaRequest
        {
            UserId = userId,
            RequestedMinutes = 25,
            IdempotencyKey = "idempotency-key-2"
        };

        var rule = UserQuotaRule.Create(Guid.NewGuid(), userId, 30, 3, 30, DateTime.UtcNow.Date, true);
        _ruleRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(rule);

        // State has already used/reserved 10 minutes, so 10 + 25 = 35 > 30 daily minutes limit
        var state = UserQuotaState.Create(Guid.NewGuid(), userId, DateTime.UtcNow.Date, 5, 5, 1, 1, 0);
        _stateRepository.GetByUserIdAndQuotaDateAsync(userId, DateTime.UtcNow.Date, Arg.Any<CancellationToken>()).Returns(state);

        // Act
        var result = await _quotaService.ReserveSessionQuotaAsync(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ReserveSessionQuotaStatus.DailyMinutesExceeded);
        result.ReservationId.Should().BeNull();

        await _rateLimitEventRepository.Received(1).AddAsync(Arg.Is<RateLimitEvent>(e => e.LimitType == RateLimitEvent.LimitTypeEnum.DailyMinutesExceeded), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReserveSessionQuotaAsync_WhenExceedingSingleSessionMinutes_ShouldReturnSingleSessionDurationExceeded()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ReserveSessionQuotaRequest
        {
            UserId = userId,
            RequestedMinutes = 40,
            IdempotencyKey = "idempotency-key-3"
        };

        var rule = UserQuotaRule.Create(Guid.NewGuid(), userId, 60, 3, 30, DateTime.UtcNow.Date, true);
        _ruleRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(rule);

        var state = UserQuotaState.Create(Guid.NewGuid(), userId, DateTime.UtcNow.Date, 0, 0, 0, 0, 0);
        _stateRepository.GetByUserIdAndQuotaDateAsync(userId, DateTime.UtcNow.Date, Arg.Any<CancellationToken>()).Returns(state);

        // Act
        var result = await _quotaService.ReserveSessionQuotaAsync(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ReserveSessionQuotaStatus.SingleSessionDurationExceeded);
        result.ReservationId.Should().BeNull();

        await _rateLimitEventRepository.Received(1).AddAsync(Arg.Is<RateLimitEvent>(e => e.LimitType == RateLimitEvent.LimitTypeEnum.SingleSessionDurationExceeded), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReserveSessionQuotaAsync_WhenIdempotentRequest_ShouldReturnExistingReservation()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new ReserveSessionQuotaRequest
        {
            UserId = userId,
            RequestedMinutes = 20,
            IdempotencyKey = "idempotency-key-4"
        };

        var rule = UserQuotaRule.Create(Guid.NewGuid(), userId, 60, 3, 30, DateTime.UtcNow.Date, true);
        _ruleRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(rule);

        var state = UserQuotaState.Create(Guid.NewGuid(), userId, DateTime.UtcNow.Date, 20, 0, 1, 0, 0);
        _stateRepository.GetByUserIdAndQuotaDateAsync(userId, DateTime.UtcNow.Date, Arg.Any<CancellationToken>()).Returns(state);

        var existingReservation = QuotaReservation.Create(
            Guid.NewGuid(), userId, request.IdempotencyKey, 20, DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.Date);

        _reservationRepository.GetByIdempotencyKeyAsync(userId, request.IdempotencyKey, Arg.Any<CancellationToken>()).Returns(existingReservation);

        // Act
        var result = await _quotaService.ReserveSessionQuotaAsync(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ReserveSessionQuotaStatus.IdempotentRepeat);
        result.ReservationId.Should().Be(existingReservation.Id);
    }

    [Fact]
    public async Task ConfirmSessionUsageAsync_WhenValidReservation_ShouldUpdateStateAndCreateUsageLog()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new ConfirmSessionUsageRequest
        {
            ReservationId = reservationId,
            ActualMinutesUsed = 15,
            CorrelationId = Guid.NewGuid()
        };

        var reservation = QuotaReservation.Create(
            reservationId, userId, "idempotency-key-5", 20, DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.Date);
        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<CancellationToken>()).Returns(reservation);

        var state = UserQuotaState.Create(Guid.NewGuid(), userId, DateTime.UtcNow.Date, 20, 0, 1, 0, 0);
        _stateRepository.GetByUserIdAndQuotaDateAsync(userId, DateTime.UtcNow.Date, Arg.Any<CancellationToken>()).Returns(state);
        _stateRepository.GetByIdAsync(state.Id, Arg.Any<CancellationToken>()).Returns(state);

        var rule = UserQuotaRule.Create(Guid.NewGuid(), userId, 60, 3, 30, DateTime.UtcNow.Date, true);
        _ruleRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(rule);

        // Act
        var result = await _quotaService.ConfirmSessionUsageAsync(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ConfirmSessionUsageStatus.Success);
        reservation.Status.Should().Be(QuotaReservation.ReservationStatus.Confirmed);

        state.ReservedMinutes.Should().Be(0);
        state.ReservedSessionCount.Should().Be(0);
        state.UsedMinutes.Should().Be(15);
        state.UsedSessionCount.Should().Be(1);

        await _usageLogRepository.Received(1).AddAsync(Arg.Is<UsageLog>(l => l.DurationMinutes == 15 && l.UserId == userId), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ConfirmSessionUsageAsync_WhenReservationAlreadyConfirmed_ShouldReturnAlreadyConfirmed()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new ConfirmSessionUsageRequest
        {
            ReservationId = reservationId,
            ActualMinutesUsed = 15,
            CorrelationId = Guid.NewGuid()
        };

        var reservation = QuotaReservation.Create(
            reservationId, userId, "idempotency-key-6", 20, DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.Date);
        reservation.Confirm(DateTime.UtcNow);
        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<CancellationToken>()).Returns(reservation);

        // Act
        var result = await _quotaService.ConfirmSessionUsageAsync(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ConfirmSessionUsageStatus.ReservationAlreadyConfirmed);
    }

    [Fact]
    public async Task CancelReservationAsync_WhenReserved_ShouldReleaseCountersAndMarkCancelled()
    {
        // Arrange
        var reservationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new CancelReservationRequest
        {
            ReservationId = reservationId
        };

        var reservation = QuotaReservation.Create(
            reservationId, userId, "idempotency-key-7", 20, DateTime.UtcNow.AddMinutes(15), DateTime.UtcNow.Date);
        _reservationRepository.GetByIdAsync(reservationId, Arg.Any<CancellationToken>()).Returns(reservation);

        var state = UserQuotaState.Create(Guid.NewGuid(), userId, DateTime.UtcNow.Date, 20, 0, 1, 0, 0);
        _stateRepository.GetByUserIdAndQuotaDateAsync(userId, DateTime.UtcNow.Date, Arg.Any<CancellationToken>()).Returns(state);
        _stateRepository.GetByIdAsync(state.Id, Arg.Any<CancellationToken>()).Returns(state);

        var rule = UserQuotaRule.Create(Guid.NewGuid(), userId, 60, 3, 30, DateTime.UtcNow.Date, true);
        _ruleRepository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(rule);

        // Act
        var result = await _quotaService.CancelReservationAsync(request, CancellationToken.None);

        // Assert
        result.Status.Should().Be(CancelReservationStatus.Success);
        reservation.Status.Should().Be(QuotaReservation.ReservationStatus.Cancelled);

        state.ReservedMinutes.Should().Be(0);
        state.ReservedSessionCount.Should().Be(0);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
