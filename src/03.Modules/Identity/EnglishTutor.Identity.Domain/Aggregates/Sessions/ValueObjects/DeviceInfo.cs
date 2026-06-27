using System.Security.Cryptography;
using System.Text;

namespace EnglishTutor.Identity.Domain.Aggregates.Sessions.ValueObjects;

/// <summary>
/// Immutable device-binding metadata captured when a <c>UserSession</c> is
/// created. Per the auth pack, only hashed user-agent and IP values are stored
/// (no raw PII). The non-PII <see cref="DeviceId"/> is stored as-is because
/// it is generated client-side.
/// </summary>
public sealed class DeviceInfo
{
    public string DeviceId { get; }
    public string? DeviceName { get; }
    public string UserAgentHash { get; }
    public string? IpAddressHash { get; }

    private DeviceInfo(
        string deviceId,
        string? deviceName,
        string userAgentHash,
        string? ipAddressHash)
    {
        DeviceId = deviceId;
        DeviceName = deviceName;
        UserAgentHash = userAgentHash;
        IpAddressHash = ipAddressHash;
    }

    /// <summary>
    /// Factory that accepts the raw user-agent and IP at the call site, then
    /// immediately hashes them so the value object never carries raw PII.
    /// </summary>
    public static DeviceInfo Create(
        string deviceId,
        string? deviceName,
        string userAgent,
        string? ipAddress)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
        {
            throw new ArgumentException("DeviceId is required.", nameof(deviceId));
        }
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            throw new ArgumentException("UserAgent is required.", nameof(userAgent));
        }

        return new DeviceInfo(
            deviceId: deviceId.Trim(),
            deviceName: string.IsNullOrWhiteSpace(deviceName) ? null : deviceName.Trim(),
            userAgentHash: ComputeHash(userAgent),
            ipAddressHash: string.IsNullOrWhiteSpace(ipAddress) ? null : ComputeHash(ipAddress));
    }

    private static string ComputeHash(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }
}
