using System.ComponentModel.DataAnnotations.Schema;
using EnglishLiveTutor.Domain.Common.Abstractions;

namespace EnglishLiveTutor.Domain.Common.Primitives;

/// <summary>
/// Base for domain entities. Existing CreatedAt/UpdatedAt public property names are
/// deliberately retained for EF Core mappings, API contracts and existing SQL data.
/// Audit values are assigned by Infrastructure on SaveChanges.
/// </summary>
public abstract class Entity<TId> : IAuditable where TId : notnull
{
    public TId Id { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public string? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? UpdatedBy { get; private set; }

    [NotMapped]
    public DateTimeOffset CreatedAtUtc => CreatedAt.ToUniversalTime();

    [NotMapped]
    public DateTimeOffset? UpdatedAtUtc => UpdatedAt?.ToUniversalTime();
}
