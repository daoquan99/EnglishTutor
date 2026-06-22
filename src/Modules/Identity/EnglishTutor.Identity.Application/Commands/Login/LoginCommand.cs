using EnglishTutor.BuildingBlocks.Application.Commands;

namespace EnglishTutor.Identity.Application.Commands.Login;

/// <summary>
/// Login command. Carries raw user input (email, password), optional client
/// IP / User-Agent / device info for audit, and returns
/// <see cref="LoginResult"/> on success.
/// <para>
/// Fields beyond Email + Password (IpAddress, UserAgent, DeviceId, DeviceName)
/// are populated by the Presentation endpoint from <c>HttpContext</c>.
/// They are NOT exposed on the HTTP DTO payload (see
/// <c>docs/api/modules/Identity.md</c>) — they are internal command fields.
/// </para>
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? IpAddress,
    string? UserAgent,
    string? DeviceId,
    string? DeviceName) : ICommand<LoginResult>;
