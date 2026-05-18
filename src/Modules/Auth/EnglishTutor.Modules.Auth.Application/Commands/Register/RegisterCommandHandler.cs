using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.DTOs;
using EnglishTutor.Modules.Auth.Application.Errors;
using EnglishTutor.Modules.Auth.Domain.Entities;
using EnglishTutor.Modules.Auth.Domain.ValueObjects;

namespace EnglishTutor.Modules.Auth.Application.Commands.Register;

public sealed class RegisterCommandHandler(
    IAuthRepository authRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IRefreshTokenGenerator refreshTokenGenerator,
    IRefreshTokenRepository refreshTokenRepository,
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenCache refreshTokenCache,
    IAuthUnitOfWork unitOfWork)
    : ICommandHandler<RegisterCommand, AuthTokenResponse>
{
    public async Task<Result<AuthTokenResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);

        if (await authRepository.EmailExistsAsync(email, cancellationToken))
        {
            return Result.Failure<AuthTokenResponse>(AuthErrors.EmailAlreadyExists);
        }

        var user = AuthUser.Register(email, request.DisplayName);
        var credential = UserCredential.Create(user.Id, passwordHasher.Hash(request.Password));
        var accessToken = jwtTokenGenerator.Generate(user);
        var session = AuthSession.Create(user.Id, request.DeviceId, request.UserAgent, request.IpAddress);
        var generatedRefreshToken = refreshTokenGenerator.Generate();
        var refreshToken = Domain.Entities.RefreshToken.Create(
            user.Id,
            session.Id,
            generatedRefreshToken.TokenHash,
            generatedRefreshToken.ExpiresAtUtc);

        await authRepository.AddAsync(user, credential, cancellationToken);
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
