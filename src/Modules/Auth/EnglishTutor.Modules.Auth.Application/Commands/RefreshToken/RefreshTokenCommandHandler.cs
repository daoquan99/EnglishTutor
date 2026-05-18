using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.DTOs;
using EnglishTutor.Modules.Auth.Application.Errors;
using EnglishTutor.Modules.Auth.Domain.Entities;
using EnglishTutor.Modules.Auth.Domain.Enums;

namespace EnglishTutor.Modules.Auth.Application.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAuthSessionRepository authSessionRepository,
    IAuthSecurityEventRepository securityEventRepository,
    IAuthRepository authRepository,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenHasher refreshTokenHasher,
    IRefreshTokenCache refreshTokenCache,
    IAuthUnitOfWork unitOfWork)
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
        var cachedTokenHash = await refreshTokenCache.GetTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
        var currentToken = await refreshTokenRepository.GetByHashAsync(requestedTokenHash, cancellationToken);

        if (currentToken is null)
        {
            await MarkSuspiciousAsync(
                session,
                AuthSecurityEventType.RefreshTokenHashMismatch,
                AuthSecurityEventSeverity.High,
                request,
                cachedTokenHash is null ? "Refresh token was not found in Redis or DB." : "Refresh token hash did not match Redis cache.",
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
            await MarkSuspiciousAsync(
                session,
                AuthSecurityEventType.RefreshTokenReuseDetected,
                AuthSecurityEventSeverity.Critical,
                request,
                "A revoked refresh token was used again.",
                cancellationToken);
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenSuspicious);
        }

        if (currentToken.IsExpired)
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
            return Result.Failure<AuthTokenResponse>(AuthErrors.UserInactive);
        }

        currentToken.MarkUsed();
        session.MarkUsed();

        var generatedRefreshToken = refreshTokenGenerator.Generate();
        var nextRefreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            session.Id,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc);
        currentToken.Revoke(nextRefreshToken.Id);
        var accessToken = jwtTokenGenerator.Generate(user);

        await refreshTokenRepository.AddAsync(nextRefreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await refreshTokenCache.StoreTokenHashAsync(
            session.Id,
            session.DeviceId,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc,
            cancellationToken);

        return new AuthTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.Token,
            session.Id,
            session.DeviceId,
            generatedRefreshToken.ExpiresAtUtc);
    }

    private async Task MarkSuspiciousAsync(
        AuthSession session,
        AuthSecurityEventType eventType,
        AuthSecurityEventSeverity severity,
        RefreshTokenCommand request,
        string reason,
        CancellationToken cancellationToken)
    {
        session.MarkSuspicious(reason);
        session.Revoke();
        await securityEventRepository.AddAsync(
            AuthSecurityEvent.Create(
                session.AuthUserId,
                session.Id,
                session.DeviceId,
                eventType,
                severity,
                request.IpAddress,
                request.UserAgent,
                $$"""{"reason":"{{reason}}"}"""),
            cancellationToken);
        await refreshTokenCache.RemoveTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
