using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent.Enums;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EnglishTutor.Modules.Auth.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAuthSessionRepository authSessionRepository,
    IAuthSecurityEventRepository securityEventRepository,
    IAuthRepository authRepository,
    IAuthPermissionRepository authPermissionRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IRefreshTokenCache refreshTokenCache,
    IAuthUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RefreshTokenCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var session = await authSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.DeviceId != request.DeviceId)
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.AuthSessionNotFound);
        }

        if (session.IsRevoked)
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenRevoked);
        }

        var requestedTokenHash = refreshTokenHasher.Hash(request.RefreshToken);
        var cacheReadResult = await refreshTokenCache.GetTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
        if (!cacheReadResult.IsAvailable)
        {
            await refreshTokenCache.RemoveTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenVerificationUnavailable);
        }

        var cachedTokenHash = cacheReadResult.TokenHash;

        if (!TokenHashesMatch(cachedTokenHash, requestedTokenHash))
        {
            await MarkSuspiciousAsync(
                session,
                AuthSecurityEventType.RefreshTokenHashMismatch,
                AuthSecurityEventSeverity.High,
                request,
                cachedTokenHash is null ? "Refresh token was not found in Redis." : "Refresh token hash did not match Redis cache.",
                cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenSuspicious);
        }

        var currentToken = await refreshTokenRepository.GetByHashAsync(requestedTokenHash, cancellationToken);

        if (currentToken is null)
        {
            await MarkSuspiciousAsync(
                session,
                AuthSecurityEventType.RefreshTokenHashMismatch,
                AuthSecurityEventSeverity.High,
                request,
                "Refresh token hash was present in Redis but was not found in DB.",
                cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenSuspicious);
        }

        if (currentToken.AuthUserId != session.AuthUserId || currentToken.SessionId != session.Id)
        {
            await MarkSuspiciousAsync(
                session,
                AuthSecurityEventType.RefreshTokenHashMismatch,
                AuthSecurityEventSeverity.Critical,
                request,
                "Refresh token belongs to a different user or session.",
                cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenSuspicious);
        }

        if (currentToken.IsRevoked)
        {
            // Concurrent-refresh race: if the cached hash STILL equals the requested hash, this
            // request is the slower twin of a successful refresh whose DB commit ran but whose
            // Redis store has not yet replaced the hash. Return a transient failure rather than
            // marking the session suspicious and kicking the legitimate user out.
            if (TokenHashesMatch(cachedTokenHash, requestedTokenHash))
            {
                return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenVerificationUnavailable);
            }

            await MarkSuspiciousAsync(
                session,
                AuthSecurityEventType.RefreshTokenReuseDetected,
                AuthSecurityEventSeverity.Critical,
                request,
                "A revoked refresh token was used again.",
                cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenSuspicious);
        }

        var utcNow = dateTimeProvider.UtcNow;

        if (currentToken.IsExpired(utcNow))
        {
            await refreshTokenCache.RemoveTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenExpired);
        }

        var user = await authRepository.GetByIdAsync(currentToken.AuthUserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.UserNotFound(currentToken.AuthUserId));
        }

        if (!user.IsActive)
        {
            // Deactivating a user must invalidate outstanding sessions/refresh tokens, otherwise the
            // suspension is silently reversible by re-enabling the account.
            currentToken.Revoke(utcNow);
            session.Revoke(utcNow);
            await refreshTokenCache.RemoveTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.UserInactive);
        }

        currentToken.MarkUsed(utcNow);
        session.MarkUsed(utcNow);

        var generatedRefreshToken = refreshTokenGenerator.Generate();
        var nextRefreshToken = global::EnglishTutor.Modules.Auth.Domain.AuthSession.Entities.RefreshToken.Create(
            user.Id,
            session.Id,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc,
            utcNow);
        currentToken.Revoke(utcNow, nextRefreshToken.Id);
        var permissions = await authPermissionRepository.GetPermissionCodesByUserIdAsync(user.Id, cancellationToken);
        var accessToken = jwtTokenGenerator.Generate(user, permissions);

        await refreshTokenRepository.AddAsync(nextRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var cacheStored = await refreshTokenCache.StoreTokenHashAsync(
            session.Id,
            session.DeviceId,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc,
            cancellationToken);

        if (!cacheStored)
        {
            var failureUtcNow = dateTimeProvider.UtcNow;
            nextRefreshToken.Revoke(failureUtcNow);
            session.Revoke(failureUtcNow);
            await refreshTokenCache.RemoveTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenVerificationUnavailable);
        }

        return new AuthTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.Token,
            session.Id,
            session.DeviceId,
            generatedRefreshToken.ExpiresAtUtc);
    }

    private static bool TokenHashesMatch(string? cachedTokenHash, string requestedTokenHash)
    {
        if (string.IsNullOrWhiteSpace(cachedTokenHash))
        {
            return false;
        }

        var cachedBytes = Encoding.UTF8.GetBytes(cachedTokenHash);
        var requestedBytes = Encoding.UTF8.GetBytes(requestedTokenHash);

        return cachedBytes.Length == requestedBytes.Length &&
            CryptographicOperations.FixedTimeEquals(cachedBytes, requestedBytes);
    }

    private async Task MarkSuspiciousAsync(
        AuthSession session,
        AuthSecurityEventType eventType,
        AuthSecurityEventSeverity severity,
        RefreshTokenCommand request,
        string reason,
        CancellationToken cancellationToken)
    {
        var utcNow = dateTimeProvider.UtcNow;
        session.MarkSuspicious(reason, utcNow);
        session.Revoke(utcNow);
        await securityEventRepository.AddAsync(
            AuthSecurityEvent.Create(
                session.AuthUserId,
                session.Id,
                session.DeviceId,
                eventType,
                severity,
                request.IpAddress,
                request.UserAgent,
                JsonSerializer.Serialize(new { reason }),
                utcNow),
            cancellationToken);
        await refreshTokenCache.RemoveTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
