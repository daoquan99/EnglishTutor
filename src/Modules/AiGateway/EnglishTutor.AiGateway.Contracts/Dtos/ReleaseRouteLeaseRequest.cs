using System;

namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Request to release/cancel an acquired AI route lease.
/// </summary>
public class ReleaseRouteLeaseRequest
{
    public Guid LeaseId { get; set; }
}
