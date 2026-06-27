using System;

namespace EnglishTutor.Identity.Application.Queries.GetUserSessions;

public sealed record UserSessionResult(
    Guid Id,
    string DeviceId,
    string? DeviceName,
    string UserAgentHash,
    string? IpAddressHash,
    DateTime CreatedAtUtc,
    DateTime LastSeenAtUtc);
