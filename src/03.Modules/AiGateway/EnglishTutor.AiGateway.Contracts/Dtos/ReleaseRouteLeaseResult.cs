namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Result of the route lease release operation.
/// </summary>
public class ReleaseRouteLeaseResult
{
    public ReleaseRouteLeaseStatus Status { get; set; }
    public string? ErrorCode { get; set; }
}
