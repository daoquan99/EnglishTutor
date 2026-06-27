using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.Identity.Application.Security;

public sealed class IdentitySecurityOptions
{
    public const string SectionName = "Identity:Security";

    [Range(1, 20)]
    public int MaxFailedLoginAttempts { get; init; } = 5;

    [Range(1, 1440)]
    public int LockoutMinutes { get; init; } = 15;

    public TimeSpan LockoutDuration => TimeSpan.FromMinutes(LockoutMinutes);
}
