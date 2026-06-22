using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace EnglishTutor.Identity.Infrastructure.Audit;

// Default implementation of IRefreshTokenReuseContextAccessor. Reads the
// current request's IP and User-Agent from IHttpContextAccessor and hashes
// them with SHA-256. Outside an HTTP request (no IHttpContextAccessor
// scope), returns null for all three.
internal sealed class HttpContextRefreshTokenReuseContextAccessor : IRefreshTokenReuseContextAccessor
{
    private const int UserAgentMaxLength = 2000;
    private const int IpMaxLength = 64;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpContextRefreshTokenReuseContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? IpAddressHash
    {
        get
        {
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();
            return string.IsNullOrWhiteSpace(ip) ? null : Hash(ip);
        }
    }

    public string? UserAgentHash
    {
        get
        {
            var ua = _httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
            if (string.IsNullOrWhiteSpace(ua)) return null;
            var truncated = ua.Length > UserAgentMaxLength ? ua.Substring(0, UserAgentMaxLength) : ua;
            return Hash(truncated);
        }
    }

    public Guid? CorrelationId
    {
        get
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx is null) return null;
            var header = ctx.Request.Headers["X-Correlation-Id"].ToString();
            return Guid.TryParse(header, out var g) ? g : (Guid?)null;
        }
    }

    private static string Hash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(bytes);
    }
}
