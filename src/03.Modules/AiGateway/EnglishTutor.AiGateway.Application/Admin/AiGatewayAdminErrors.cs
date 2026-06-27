using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.AiGateway.Application.Admin;

public static class AiGatewayAdminErrors
{
    public static Error Validation(string detail) => Error.Validation("AiGateway.Validation", detail);
    public static Error NotFound(string what) => Error.NotFound("AiGateway.NotFound", $"{what} not found.");
    public static Error Conflict(string detail) => Error.Conflict("AiGateway.Conflict", detail);
}
