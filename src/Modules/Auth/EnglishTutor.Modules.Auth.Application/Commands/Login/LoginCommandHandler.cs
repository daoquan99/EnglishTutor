using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.DTOs;
using EnglishTutor.Modules.Auth.Application.Errors;
using EnglishTutor.Modules.Auth.Domain.Entities;
using EnglishTutor.Modules.Auth.Domain.ValueObjects;

namespace EnglishTutor.Modules.Auth.Application.Commands.Login;

public sealed class LoginCommandHandler(
    IAuthRepository authRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenCache refreshTokenCache,
    IAuthUnitOfWork unitOfWork)
    : ICommandHandler<LoginCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var user = await authRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
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

        var accessToken = jwtTokenGenerator.Generate(user);
        var session = AuthSession.Create(user.Id, request.DeviceId, request.UserAgent, request.IpAddress);
        var generatedRefreshToken = refreshTokenGenerator.Generate();
        var refreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            session.Id,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc);

        await authSessionRepository.AddAsync(session, cancellationToken);
        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
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
}
