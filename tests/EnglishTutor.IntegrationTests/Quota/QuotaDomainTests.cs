using System;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using FluentAssertions;
using Xunit;

namespace EnglishTutor.IntegrationTests.Quota;

public class QuotaDomainTests
{
    [Fact]
    public void UserQuotaRule_Create_WithValidArgs_ShouldInitializeCorrectly()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var effectiveDate = DateTime.UtcNow.Date;

        // Act
        var rule = UserQuotaRule.Create(id, userId, 120, 5, 45, effectiveDate, true);

        // Assert
        rule.Id.Should().Be(id);
        rule.UserId.Should().Be(userId);
        rule.DailyMaxSessionMinutes.Should().Be(120);
        rule.DailyMaxSessions.Should().Be(5);
        rule.MaxSingleSessionDuration.Should().Be(45);
        rule.EffectiveDate.Should().Be(effectiveDate);
        rule.IsActive.Should().BeTrue();
        rule.Version.Should().Be(1);
    }

    [Theory]
    [InlineData(0, 5, 45)]
    [InlineData(120, 0, 45)]
    [InlineData(120, 5, 0)]
    public void UserQuotaRule_Create_WithInvalidLimits_ShouldThrowArgumentOutOfRangeException(
        int dailyMinutes, int dailySessions, int singleDuration)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            UserQuotaRule.Create(Guid.NewGuid(), Guid.NewGuid(), dailyMinutes, dailySessions, singleDuration, DateTime.UtcNow, true));
    }

    [Fact]
    public void UserQuotaState_AddAndConsumeReservedMinutes_ShouldModifyCountersCorrectly()
    {
        // Arrange
        var state = UserQuotaState.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date, 0, 0, 0, 0, 0);

        // Act
        state.AddReservedMinutes(30);

        // Assert
        state.ReservedMinutes.Should().Be(30);

        // Act 2
        state.ConsumeReservedMinutes(10);

        // Assert 2
        state.ReservedMinutes.Should().Be(20);
    }

    [Fact]
    public void UserQuotaState_ConsumeReservedMinutes_ExceedingAvailable_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var state = UserQuotaState.Create(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.Date, 10, 0, 0, 0, 0);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => state.ConsumeReservedMinutes(15));
    }

    [Fact]
    public void QuotaReservation_Confirm_ShouldChangeStatusAndSetConfirmedAt()
    {
        // Arrange
        var reservation = QuotaReservation.Create(
            Guid.NewGuid(), Guid.NewGuid(), "idempotency-key", 30, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow.Date);
        var confirmTime = DateTime.UtcNow;

        // Act
        reservation.Confirm(confirmTime);

        // Assert
        reservation.Status.Should().Be(QuotaReservation.ReservationStatus.Confirmed);
        reservation.ConfirmedAtUtc.Should().Be(confirmTime);
        reservation.Version.Should().Be(2);
    }

    [Fact]
    public void QuotaReservation_Confirm_WhenAlreadyConfirmed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var reservation = QuotaReservation.Create(
            Guid.NewGuid(), Guid.NewGuid(), "idempotency-key", 30, DateTime.UtcNow.AddMinutes(30), DateTime.UtcNow.Date);
        reservation.Confirm(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => reservation.Confirm(DateTime.UtcNow));
    }
}
