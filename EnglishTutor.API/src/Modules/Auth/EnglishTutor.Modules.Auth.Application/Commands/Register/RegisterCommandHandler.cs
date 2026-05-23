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

namespace EnglishTutor.Modules.Auth.Application.Commands.Register;

public sealed class RegisterCommandHandler(
    IAuthRepository authRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenCache refreshTokenCache,
    IAuthUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RegisterCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (await authRepository.EmailExistsAsync(email, cancellationToken))
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.EmailAlreadyExists);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var user = AuthUser.Register(email, request.DisplayName, utcNow);
        var credential = UserCredential.Create(user.Id, passwordHasher.Hash(request.Password), utcNow);
        var accessToken = jwtTokenGenerator.Generate(user, []);
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

        await authRepository.AddAsync(user, credential, cancellationToken);
        await authSessionRepository.AddAsync(session, cancellationToken);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            if (await authRepository.EmailExistsAsync(email, cancellationToken))
            {
                return Result.Failure<AuthTokenResponse>(AuthErrors.EmailAlreadyExists);
            }

            throw;
        }

        return new AuthTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            generatedRefreshToken.Token,
            session.Id,
            session.DeviceId,
            generatedRefreshToken.ExpiresAtUtc);
    }
}
