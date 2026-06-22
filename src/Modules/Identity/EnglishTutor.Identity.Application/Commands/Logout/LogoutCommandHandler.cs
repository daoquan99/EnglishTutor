using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;

namespace EnglishTutor.Identity.Application.Commands.Logout;

/// <summary>
/// Handles <see cref="LogoutCommand"/>: looks up the supplied refresh token
/// by hash and revokes its family. Idempotent — when no matching token is
/// found the command succeeds silently (matches the pre-Slice 2.6
/// behavior).
/// <para>
/// Persistence-agnostic: depends only on <c>IUserSessionRepository</c> and
/// <c>IIdentityUnitOfWork</c>. It does NOT reference <c>the DbContext abstraction</c>
/// or any other Infrastructure type.
/// </para>
/// <para>
/// NOTE: A UserSession / RefreshTokenFamily is NOT yet created by
/// <c>LoginCommandHandler</c>, so the rich <c>session.Revoke(...)</c>
/// path (which walks the family and revokes sibling tokens) is not used
/// here. The handler instead mutates the matched token directly via
/// <c>RefreshToken.RevokeFamily</c> — this matches the pre-Slice 2.6
/// behavior exactly. Once the session/family schema is introduced in
/// Slice 2.7, this handler should switch to the session-based revoke.
/// </para>
/// </summary>
public sealed class LogoutCommandHandler : ICommandHandler<LogoutCommand>
{
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IRefreshTokenHasher _refreshTokenHasher;

    public LogoutCommandHandler(
        IUserSessionRepository userSessionRepository,
        IIdentityUnitOfWork unitOfWork,
        IRefreshTokenHasher refreshTokenHasher)
    {
        _userSessionRepository = userSessionRepository;
        _unitOfWork = unitOfWork;
        _refreshTokenHasher = refreshTokenHasher;
    }

    public async Task<Result> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var tokenHash = RefreshTokenHash.FromHex(_refreshTokenHasher.Hash(request.RefreshToken));
        var snapshot = await _userSessionRepository.FindByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (snapshot is null)
        {
            // Idempotent: if the token is unknown (or the session/family is
            // already gone), logout succeeds silently.
            return Result.Success();
        }

        // Revoke via the UserSession aggregate (cascades the family revocation).
        snapshot.Session.Revoke(DateTime.UtcNow, revokedByUserId: null, reason: "user_logout");

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}