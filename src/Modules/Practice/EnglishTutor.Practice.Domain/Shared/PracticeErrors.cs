using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Practice.Domain.Errors;

public static class PracticeErrors
{
    public static Error ScenarioNotFound(System.Guid scenarioId) =>
        Error.NotFound("Practice.ScenarioNotFound", $"The scenario with ID {scenarioId} was not found in Practice read models.");

    public static Error SessionNotFound(System.Guid sessionId) =>
        Error.NotFound("Practice.SessionNotFound", $"The practice session with ID {sessionId} was not found.");

    public static Error NotSessionOwner() =>
        Error.Forbidden("Practice.NotSessionOwner", "You do not own this practice session.");

    public static Error SessionNotActive(string status) =>
        Error.Conflict("Practice.SessionNotActive", $"The session is in state '{status}' and cannot receive messages.");

    public static Error QuotaReservationFailed(string details) =>
        Error.Conflict("Practice.QuotaReservationFailed", $"Quota reservation failed: {details}");

    public static Error AiRouteLeaseFailed(string details) =>
        Error.Conflict("Practice.AiRouteLeaseFailed", $"AI route lease creation failed: {details}");
}
