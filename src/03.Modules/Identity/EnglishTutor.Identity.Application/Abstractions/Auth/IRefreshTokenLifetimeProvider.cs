namespace EnglishTutor.Identity.Application.Abstractions.Auth;

/// <summary>
/// Provides the configured refresh-token lifetime as a <see cref="TimeSpan"/>.
/// Application code does NOT need to know which Infrastructure option class
/// supplies the value.
/// </summary>
public interface IRefreshTokenLifetimeProvider
{
    TimeSpan RefreshTokenLifetime { get; }
}
