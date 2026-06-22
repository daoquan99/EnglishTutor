using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Options;

/// <summary>
/// Strongly-typed options for the Redis connection.
/// Bound from configuration section <c>Redis</c>.
/// </summary>
public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// StackExchange.Redis configuration string (e.g. "localhost:6379" or
    /// "redis-0.redis:6379,redis-1.redis:6379" for cluster/sentinel setups).
    /// </summary>
    [Required]
    [MinLength(3)]
    public string Configuration { get; set; } = "localhost:6379";
}
