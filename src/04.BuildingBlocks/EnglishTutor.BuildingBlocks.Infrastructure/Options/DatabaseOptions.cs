using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Options;

/// <summary>
/// Strongly-typed options for the PostgreSQL connection string.
/// Bound from configuration section <c>ConnectionStrings</c>.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    /// <summary>
    /// EF Core / Npgsql connection string. Production should never set this
    /// via appsettings; use environment variables, user-secrets, or secret manager.
    /// </summary>
    [Required]
    [MinLength(10)]
    public string Default { get; set; } = string.Empty;
}
