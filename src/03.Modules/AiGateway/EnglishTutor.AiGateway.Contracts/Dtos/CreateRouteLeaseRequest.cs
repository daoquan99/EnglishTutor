using System;

namespace EnglishTutor.AiGateway.Contracts.Dtos;

/// <summary>
/// Request DTO to create a new AI route lease.
/// </summary>
public class CreateRouteLeaseRequest
{
    public Guid UserId { get; set; }
    public string ActivityType { get; set; } = default!;
    public string RequiredCapability { get; set; } = "content-generation";
    public string TopicCode { get; set; } = default!;
    public string ScenarioCode { get; set; } = default!;
    public string IdempotencyKey { get; set; } = default!;
    public Guid? CorrelationId { get; set; }
}
