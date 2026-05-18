namespace EnglishTutor.Modules.Auth.Application.Abstractions;

public interface IRefreshTokenCache
{
    Task<string?> GetTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken);

    Task StoreTokenHashAsync(
        Guid sessionId,
        string deviceId,
        string tokenHash,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken);

    Task RemoveTokenHashAsync(Guid sessionId, string deviceId, CancellationToken cancellationToken);
}
