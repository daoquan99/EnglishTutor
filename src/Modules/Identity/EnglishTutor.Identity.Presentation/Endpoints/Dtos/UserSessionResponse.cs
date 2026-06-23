using System;

namespace EnglishTutor.Identity.Presentation.Endpoints.Dtos;

public sealed record UserSessionResponse(
    Guid Id,
    string DeviceId,
    string? DeviceName,
    string UserAgentHash,
    string? IpAddressHash,
    DateTime CreatedAtUtc,
    DateTime LastSeenAtUtc);
