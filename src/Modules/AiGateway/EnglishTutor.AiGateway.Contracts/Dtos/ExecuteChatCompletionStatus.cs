namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Status codes for the secure chat completion execution.
/// </summary>
public enum ExecuteChatCompletionStatus
{
    Success = 1,
    LeaseNotFound = 2,
    LeaseExpired = 3,
    InvalidLeaseState = 4,
    ProviderRateLimited = 5,
    ProviderAuthFailed = 6,
    ProviderTimeout = 7,
    ValidationError = 8,
    ConfigurationMissing = 9
}
