using EnglishTutor.Identity.Application.Abstractions.Auth;
using EnglishTutor.Identity.Infrastructure.Security.Options;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Identity.Infrastructure.Security;

/// <summary>
/// Infrastructure implementation of <see cref="IRefreshTokenLifetimeProvider"/>.
/// Reads the configured refresh-token lifetime from <see cref="JwtOptions"/>.
/// </summary>
internal sealed class RefreshTokenLifetimeProvider : IRefreshTokenLifetimeProvider
{
    private readonly JwtOptions _options;

    public RefreshTokenLifetimeProvider(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public TimeSpan RefreshTokenLifetime =>
        TimeSpan.FromDays(_options.RefreshTokenDays);
}
