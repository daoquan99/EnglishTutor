namespace EnglishTutor.BuildingBlocks.Presentation.Responses;

public static class ApiErrorCodes
{
    public const string InvalidRequest = "request.invalid";
    public const string Unauthorized = "auth.unauthorized";
    public const string Forbidden = "auth.forbidden";
    public const string NotFound = "resource.not_found";
    public const string RateLimited = "request.rate_limited";
    public const string Unexpected = "server.unexpected";
}
