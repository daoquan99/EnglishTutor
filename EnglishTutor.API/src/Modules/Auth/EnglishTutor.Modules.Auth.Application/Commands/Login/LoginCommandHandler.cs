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
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;

namespace EnglishTutor.Modules.Auth.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IAuthRepository authRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IAuthSessionRepository authSessionRepository,
    IAuthPermissionRepository authPermissionRepository,
    IRefreshTokenCache refreshTokenCache,
    IAuthUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<LoginCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await authRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            _ = passwordHasher.Hash(request.Password);
            return Result.Failure<AuthTokenResponse>(AuthErrors.InvalidCredentials);
        }

        if (!user.IsActive)
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.UserInactive);
        }

        var credential = await authRepository.GetCredentialByUserIdAsync(user.Id, cancellationToken);
        if (credential is null || !passwordHasher.Verify(request.Password, credential.HashedPassword))
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.InvalidCredentials);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var permissions = await authPermissionRepository.GetPermissionCodesByUserIdAsync(user.Id, cancellationToken);
        var accessToken = jwtTokenGenerator.Generate(user, permissions);
        var session = AuthSession.Create(user.Id, request.DeviceId, request.UserAgent, request.IpAddress, utcNow);
        var generatedRefreshToken = refreshTokenGenerator.Generate();
        var refreshToken = global::EnglishTutor.Modules.Auth.Domain.AuthSession.Entities.RefreshToken.Create(
            user.Id,
            session.Id,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc,
            utcNow);

        var cacheStored = await refreshTokenCache.StoreTokenHashAsync(
            session.Id,
            session.DeviceId,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc,
            cancellationToken);

        if (!cacheStored)
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.RefreshTokenVerificationUnavailable);
        }

        await authSessionRepository.AddAsync(session, cancellationToken);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.Token,
            session.Id,
            session.DeviceId,
            generatedRefreshToken.ExpiresAtUtc);
    }
}
