namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Status codes for the route lease creation operation.
/// </summary>
public enum CreateRouteLeaseStatus
{
    Success = 1,
    RouteNoMatch = 2,
    ModelUnavailable = 3,
    ProviderKeyUnavailable = 4,
    IdempotentRepeat = 5,
    ValidationError = 6,
    ConfigurationMissing = 7
}
