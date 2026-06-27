using System;

namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Result of the route lease creation operation.
/// </summary>
public class CreateRouteLeaseResult
{
    public Guid? LeaseId { get; set; }
    public string? ModelCode { get; set; }
    public string? ProviderCode { get; set; }
    public CreateRouteLeaseStatus Status { get; set; }
    public DateTime? ExpiryAtUtc { get; set; }
    public string? ErrorCode { get; set; }
}
