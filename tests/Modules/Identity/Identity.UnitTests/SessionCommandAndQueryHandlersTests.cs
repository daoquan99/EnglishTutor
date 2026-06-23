using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Commands.LogoutAll;
using EnglishTutor.Identity.Application.Commands.RevokeSession;
using EnglishTutor.Identity.Application.Queries.GetUserSessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace EnglishTutor.Identity.UnitTests;

public class SessionCommandAndQueryHandlersTests
{
    private readonly FakeUserSessionRepository _repository;
    private readonly FakeIdentityUnitOfWork _unitOfWork;

    public SessionCommandAndQueryHandlersTests()
    {
        _repository = new FakeUserSessionRepository();
        _unitOfWork = new FakeIdentityUnitOfWork();
    }

    [Fact]
    public async Task GetUserSessionsQueryHandler_Should_Return_Active_Sessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session1 = UserSession.Create(userId, DeviceInfo.Create("d1", "Name1", "ua1", "ip1"), DateTime.UtcNow);
        var session2 = UserSession.Create(userId, DeviceInfo.Create("d2", "Name2", "ua2", "ip2"), DateTime.UtcNow);
        // Revoked session
        var session3 = UserSession.Create(userId, DeviceInfo.Create("d3", "Name3", "ua3", "ip3"), DateTime.UtcNow);
        session3.Revoke(DateTime.UtcNow, userId, "revoked");

        _repository.Sessions.Add(session1);
        _repository.Sessions.Add(session2);

        var query = new GetUserSessionsQuery(userId);
        var handler = new GetUserSessionsQueryHandler(_repository);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(s => s.Id).Should().Contain(new[] { session1.Id, session2.Id });
    }

    [Fact]
    public async Task LogoutAllCommandHandler_Should_Revoke_All_Active_Sessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session1 = UserSession.Create(userId, DeviceInfo.Create("d1", "Name1", "ua1", "ip1"), DateTime.UtcNow);
        var session2 = UserSession.Create(userId, DeviceInfo.Create("d2", "Name2", "ua2", "ip2"), DateTime.UtcNow);

        _repository.Sessions.Add(session1);
        _repository.Sessions.Add(session2);

        var command = new LogoutAllCommand(userId);
        var handler = new LogoutAllCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session1.RevokedAtUtc.Should().NotBeNull();
        session1.RevokedReason.Should().Be("logout_all");
        session2.RevokedAtUtc.Should().NotBeNull();
        session2.RevokedReason.Should().Be("logout_all");
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task RevokeSessionCommandHandler_Should_Revoke_Target_Session_If_Owned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var session = UserSession.Create(userId, DeviceInfo.Create("d1", "Name1", "ua1", "ip1"), DateTime.UtcNow);
        _repository.Sessions.Add(session);

        var command = new RevokeSessionCommand(session.Id, userId, IsAdmin: false);
        var handler = new RevokeSessionCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.RevokedAtUtc.Should().NotBeNull();
        session.RevokedReason.Should().Be("session_revocation");
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task RevokeSessionCommandHandler_Should_Revoke_Foreign_Session_If_Admin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var session = UserSession.Create(userId, DeviceInfo.Create("d1", "Name1", "ua1", "ip1"), DateTime.UtcNow);
        _repository.Sessions.Add(session);

        var command = new RevokeSessionCommand(session.Id, adminUserId, IsAdmin: true);
        var handler = new RevokeSessionCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        session.RevokedAtUtc.Should().NotBeNull();
        _unitOfWork.SavedChangesCount.Should().Be(1);
    }

    [Fact]
    public async Task RevokeSessionCommandHandler_Should_Return_NotFound_If_Foreign_Session_And_Not_Admin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var session = UserSession.Create(userId, DeviceInfo.Create("d1", "Name1", "ua1", "ip1"), DateTime.UtcNow);
        _repository.Sessions.Add(session);

        var command = new RevokeSessionCommand(session.Id, otherUserId, IsAdmin: false);
        var handler = new RevokeSessionCommandHandler(_repository, _unitOfWork);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Identity.SessionNotFound");
        session.RevokedAtUtc.Should().BeNull();
        _unitOfWork.SavedChangesCount.Should().Be(0);
    }
}

internal class FakeUserSessionRepository : IUserSessionRepository
{
    public List<UserSession> Sessions { get; } = new();

    public Task<UserSessionWithActiveToken?> FindByRefreshTokenHashAsync(RefreshTokenHash tokenHash, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshToken?> FindByHashAsync(RefreshTokenHash tokenHash, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<UserSession?> GetByIdAsync(Guid sessionId, CancellationToken ct)
    {
        var session = Sessions.FirstOrDefault(s => s.Id == sessionId);
        return Task.FromResult(session);
    }

    public Task<IReadOnlyList<UserSession>> GetActiveSessionsByUserIdAsync(Guid userId, CancellationToken ct)
    {
        IReadOnlyList<UserSession> filtered = Sessions
            .Where(s => s.UserId == userId && s.RevokedAtUtc == null)
            .ToList();
        return Task.FromResult(filtered);
    }

    public void Add(UserSession session)
    {
        Sessions.Add(session);
    }

    public void Add(RefreshTokenFamily family) { }
    public void Add(RefreshToken token) { }

    public Task<int> PurgeExpiredRefreshTokensAsync(DateTime expiredBeforeUtc, CancellationToken cancellationToken)
    {
        return Task.FromResult(0);
    }
}

internal class FakeIdentityUnitOfWork : IIdentityUnitOfWork
{
    public int SavedChangesCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SavedChangesCount++;
        return Task.FromResult(1);
    }
}
