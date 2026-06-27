namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Status codes for the route usage confirmation operation.
/// </summary>
public enum ConfirmRouteUsageStatus
{
    Success = 1,
    LeaseNotFound = 2,
    LeaseExpired = 3,
    InvalidLeaseState = 4,
    ValidationError = 5
}
