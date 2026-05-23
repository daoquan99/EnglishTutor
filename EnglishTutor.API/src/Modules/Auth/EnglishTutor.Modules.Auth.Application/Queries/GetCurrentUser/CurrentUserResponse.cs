using System.Text.Json.Serialization;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetCurrentUser;

public sealed record CurrentUserResponse
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Roles { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Permissions { get; init; }
}
