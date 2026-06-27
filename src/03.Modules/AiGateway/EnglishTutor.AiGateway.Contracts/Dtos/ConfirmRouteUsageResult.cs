namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Result of the route usage confirmation operation.
/// </summary>
public class ConfirmRouteUsageResult
{
    public ConfirmRouteUsageStatus Status { get; set; }
    public string? ErrorCode { get; set; }
}
