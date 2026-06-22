using System.ComponentModel.DataAnnotations;

namespace EnglishTutor.BuildingBlocks.Infrastructure.Options;

/// <summary>
/// Strongly-typed options for the RabbitMQ broker connection.
/// Bound from configuration section <c>RabbitMq</c>.
/// </summary>
public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    [Required]
    [MinLength(1)]
    public string HostName { get; set; } = "localhost";

    [Range(1, 65535)]
    public int Port { get; set; } = 5672;

    [Required]
    [MinLength(1)]
    public string UserName { get; set; } = "guest";

    [Required]
    [MinLength(1)]
    public string Password { get; set; } = "guest";

    [Required]
    [MinLength(1)]
    public string VirtualHost { get; set; } = "/";
}
