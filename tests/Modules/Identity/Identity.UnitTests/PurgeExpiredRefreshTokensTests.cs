using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Commands.PurgeExpiredRefreshTokens;
using EnglishTutor.Identity.Domain.Aggregates.Sessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EnglishTutor.Identity.UnitTests;

public class PurgeExpiredRefreshTokensTests
{
    private readonly PurgeExpiredTokensFakeRepository _repository;
    private readonly FakeIdentityUnitOfWork _unitOfWork;
    private readonly FakeDateTimeProvider _clock;
    private readonly PurgeExpiredRefreshTokensCommandHandler _handler;

    public PurgeExpiredRefreshTokensTests()
    {
        _repository = new PurgeExpiredTokensFakeRepository();
        _unitOfWork = new FakeIdentityUnitOfWork();
        _clock = new FakeDateTimeProvider(new DateTime(2026, 6, 24, 12, 0, 0, DateTimeKind.Utc));
        _handler = new PurgeExpiredRefreshTokensCommandHandler(
            _repository,
            _unitOfWork,
            _clock,
            NullLogger<PurgeExpiredRefreshTokensCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handler_Should_Only_Purge_Tokens_Expired_Before_Threshold()
    {
        // Arrange
        // Current time: 2026-06-24 12:00:00 UTC
        // Retention: 30 days. Cutoff = 2026-05-25 12:00:00 UTC.
        // We only purge tokens where ExpiresAtUtc < Cutoff.
        
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        // 1. Expired before cutoff (eligible for purge)
        var token1 = RefreshToken.Issue(
            userId, 
            familyId, 
            "hash1", 
            new DateTime(2026, 5, 24, 12, 0, 0, DateTimeKind.Utc), // Expired 2026-05-24
            null); 

        // 2. Expired after cutoff (retained)
        var token2 = RefreshToken.Issue(
            userId, 
            familyId, 
            "hash2", 
            new DateTime(2026, 6, 1, 12, 0, 0, DateTimeKind.Utc), // Expired 2026-06-01
            null); 

        // 3. Active/unexpired (retained)
        var token3 = RefreshToken.Issue(
            userId, 
            familyId, 
            "hash3", 
            new DateTime(2026, 7, 1, 12, 0, 0, DateTimeKind.Utc), // Expires 2026-07-01
            null); 

        _repository.Tokens.Add(token1);
        _repository.Tokens.Add(token2);
        _repository.Tokens.Add(token3);

        var command = new PurgeExpiredRefreshTokensCommand(30);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(1); // 1 token purged
        _repository.Tokens.Should().HaveCount(2);
        _repository.Tokens.Should().Contain(new[] { token2, token3 });
        _repository.Tokens.Should().NotContain(token1);
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task Handler_Should_Be_Idempotent()
    {
        // Arrange
        var familyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token1 = RefreshToken.Issue(
            userId, 
            familyId, 
            "hash1", 
            new DateTime(2026, 5, 24, 12, 0, 0, DateTimeKind.Utc), // Expired before cutoff
            null);

        _repository.Tokens.Add(token1);

        var command = new PurgeExpiredRefreshTokensCommand(30);

        // Act - First Run
        var result1 = await _handler.Handle(command, CancellationToken.None);

        // Act - Second Run
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result1.Value.Should().Be(1);
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be(0); // Second run deletes nothing
        _repository.Tokens.Should().BeEmpty();
    }
}

internal class PurgeExpiredTokensFakeRepository : IUserSessionRepository
{
    public List<RefreshToken> Tokens { get; } = new();

    public Task<int> PurgeExpiredRefreshTokensAsync(DateTime expiredBeforeUtc, CancellationToken cancellationToken)
    {
        var toRemove = Tokens.Where(t => t.ExpiresAtUtc < expiredBeforeUtc).ToList();
        foreach (var t in toRemove)
        {
            Tokens.Remove(t);
        }
        return Task.FromResult(toRemove.Count);
    }

    public Task<UserSessionWithActiveToken?> FindByRefreshTokenHashAsync(RefreshTokenHash tokenHash, CancellationToken ct) => throw new NotImplementedException();
    public Task<RefreshToken?> FindByHashAsync(RefreshTokenHash tokenHash, CancellationToken ct) => throw new NotImplementedException();
    public Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken ct) => throw new NotImplementedException();
    public Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct) => throw new NotImplementedException();
    public void Add(UserSession session) => throw new NotImplementedException();
    public void Add(RefreshTokenFamily family) => throw new NotImplementedException();
    public void Add(RefreshToken token) => throw new NotImplementedException();
}

internal class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow { get; }

    public FakeDateTimeProvider(DateTime utcNow)
    {
        UtcNow = utcNow;
    }
}
