using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;

namespace EnglishTutor.Modules.Auth.Application.Commands.Logout;

public sealed class LogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IAuthSessionRepository authSessionRepository,
    IRefreshTokenHasher refreshTokenHasher,
    IRefreshTokenCache refreshTokenCache,
    IAuthUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<LogoutCommand>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var session = await authSessionRepository.GetByIdAsync(request.SessionId, cancellationToken);
        if (session is null || session.DeviceId != request.DeviceId)
        {
            return Result.Failure(AuthErrors.AuthSessionNotFound);
        }

        var tokenHash = refreshTokenHasher.Hash(request.RefreshToken);
        var refreshToken = await refreshTokenRepository.GetByHashAsync(tokenHash, cancellationToken);
        if (refreshToken is null || refreshToken.AuthUserId != session.AuthUserId || refreshToken.SessionId != session.Id)
        {
            return Result.Failure(AuthErrors.RefreshTokenNotFound);
        }

        var utcNow = dateTimeProvider.UtcNow;
        refreshToken.Revoke(utcNow);
        session.Revoke(utcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await refreshTokenCache.RemoveTokenHashAsync(session.Id, session.DeviceId, cancellationToken);
        return Result.Success();
    }
}
