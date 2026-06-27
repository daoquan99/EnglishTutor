namespace EnglishTutor.AiGateway.Domain.Aggregates.AiRouteLease;

/// <summary>
/// Status states of an AI Route Lease.
/// </summary>
public enum AiRouteLeaseStatus
{
    Reserved = 1,
    Confirmed = 2,
    Released = 3,
    Expired = 4
}
