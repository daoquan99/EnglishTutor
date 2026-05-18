namespace EnglishTutor.BuildingBlocks.Application.Results;

public sealed record Error(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? Details = null)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    public static Error NotFound(string entityName, object id) =>
        new("Error.NotFound", $"{entityName} with id '{id}' was not found.");

    public static Error Validation(
        string message,
        IReadOnlyDictionary<string, string[]>? details = null) =>
        new("Error.Validation", message, details);

    public static Error Conflict(string message) =>
        new("Error.Conflict", message);

    public static Error Unauthorized(string message = "Unauthorized.") =>
        new("Error.Unauthorized", message);

    public static Error Forbidden(string message = "Forbidden.") =>
        new("Error.Forbidden", message);
}
