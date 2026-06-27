namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Status codes for the route lease release operation.
/// </summary>
public enum ReleaseRouteLeaseStatus
{
    Success = 1,
    LeaseNotFound = 2,
    InvalidLeaseState = 3,
    ValidationError = 4
}
