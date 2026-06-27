using EnglishTutor.BuildingBlocks.Domain.Results;

namespace EnglishTutor.Realtime.Application.Errors;

public static class RealtimeErrors
{
    public static Error SessionAccessDenied => Error.Validation("realtime.session.access_denied", "Access to the practice session was denied.");
    public static Error ConnectionNotFound => Error.NotFound("realtime.connection.not_found", "The specified connection was not found in the registry.");
}
