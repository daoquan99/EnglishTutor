using System;
using System.Linq;
using System.Threading.Tasks;
using EnglishTutor.Quota.Domain.Aggregates.QuotaReservation;
using EnglishTutor.Quota.Domain.Aggregates.RateLimitEvent;
using EnglishTutor.Quota.Domain.Aggregates.UsageLog;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaRule;
using EnglishTutor.Quota.Domain.Aggregates.UserQuotaState;
using EnglishTutor.Quota.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EnglishTutor.IntegrationTests.Quota;

[Collection("EnglishTutorIntegrationTests")]
public class QuotaPersistenceTests
{
    private sealed class QuotaTestFactory : IntegrationTestFactory
    {
    }

    [Fact]
    public async Task Creating_Quota_Entities_Should_Stamp_Audit_Fields()
    {
        // Arrange
        await using var factory = new QuotaTestFactory();
        using var scope = factory.Services.CreateScope();
        var quotaDb = scope.ServiceProvider.GetRequiredService<QuotaDbContext>();
        await quotaDb.Database.MigrateAsync();

        var userId = Guid.NewGuid();
        var rule = UserQuotaRule.Create(
            Guid.NewGuid(),
            userId,
            60,
            3,
            30,
            DateTime.UtcNow.Date,
            true);

        // Act
        await quotaDb.UserQuotaRules.AddAsync(rule);
        await quotaDb.SaveChangesAsync();

        // Assert
        var savedRule = await quotaDb.UserQuotaRules.FindAsync(rule.Id);
        savedRule.Should().NotBeNull();
        savedRule!.CreatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
        savedRule.UpdatedAtUtc.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task Deleting_QuotaReservation_Should_Soft_Delete_And_Apply_Query_Filters()
    {
        // Arrange
        await using var factory = new QuotaTestFactory();
        using var scope = factory.Services.CreateScope();
        var quotaDb = scope.ServiceProvider.GetRequiredService<QuotaDbContext>();
        await quotaDb.Database.MigrateAsync();

        var userId = Guid.NewGuid();
        var reservation = QuotaReservation.Create(
            Guid.NewGuid(),
            userId,
            "test-idempotency-key",
            20,
            DateTime.UtcNow.AddMinutes(30),
            DateTime.UtcNow.Date);

        await quotaDb.QuotaReservations.AddAsync(reservation);
        await quotaDb.SaveChangesAsync();

        // Act: delete reservation using standard EF Remove
        quotaDb.QuotaReservations.Remove(reservation);
        await quotaDb.SaveChangesAsync();

        // Assert: should be soft deleted and excluded by default query filter
        var queryableReservation = await quotaDb.QuotaReservations
            .FirstOrDefaultAsync(r => r.Id == reservation.Id);
        queryableReservation.Should().BeNull();

        // Should be retrievable when ignoring query filters
        var softDeletedReservation = await quotaDb.QuotaReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == reservation.Id);
        
        softDeletedReservation.Should().NotBeNull();
        softDeletedReservation!.IsDeleted.Should().BeTrue();
        softDeletedReservation.DeletedAtUtc.Should().NotBeNull();
        softDeletedReservation.DeletedAtUtc.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task UsageLog_And_RateLimitEvent_Should_Hard_Delete()
    {
        // Arrange
        await using var factory = new QuotaTestFactory();
        using var scope = factory.Services.CreateScope();
        var quotaDb = scope.ServiceProvider.GetRequiredService<QuotaDbContext>();
        await quotaDb.Database.MigrateAsync();

        var userId = Guid.NewGuid();
        var reservationId = Guid.NewGuid();
        var usageLog = UsageLog.Create(
            Guid.NewGuid(),
            userId,
            reservationId,
            15,
            DateTime.UtcNow,
            "shadowing");

        var rateLimitEvent = RateLimitEvent.Create(
            Guid.NewGuid(),
            userId,
            "exceeded",
            RateLimitEvent.LimitTypeEnum.DailyMinutesExceeded,
            45,
            DateTime.UtcNow);

        await quotaDb.UsageLogs.AddAsync(usageLog);
        await quotaDb.RateLimitEvents.AddAsync(rateLimitEvent);
        await quotaDb.SaveChangesAsync();

        // Act: perform a standard delete
        quotaDb.UsageLogs.Remove(usageLog);
        quotaDb.RateLimitEvents.Remove(rateLimitEvent);
        await quotaDb.SaveChangesAsync();

        // Assert: verify both are completely gone from the DB (hard delete)
        var queryUsageLog = await quotaDb.UsageLogs.FindAsync(usageLog.Id);
        queryUsageLog.Should().BeNull();

        var queryRateLimitEvent = await quotaDb.RateLimitEvents.FindAsync(rateLimitEvent.Id);
        queryRateLimitEvent.Should().BeNull();
    }
}
