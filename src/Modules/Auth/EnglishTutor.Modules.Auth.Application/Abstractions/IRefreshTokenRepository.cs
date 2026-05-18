using EnglishTutor.Modules.Auth.Domain.Entities;

namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
}
