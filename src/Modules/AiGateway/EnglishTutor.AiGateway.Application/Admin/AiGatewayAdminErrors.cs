using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.AiGateway.Application.Admin;

public static class AiGatewayAdminErrors
{
    public static Error Validation(string detail) => new("AiGateway.Validation", detail);
    public static Error NotFound(string what) => new("AiGateway.NotFound", $"{what} not found.");
    public static Error Conflict(string detail) => new("AiGateway.Conflict", detail);
}
