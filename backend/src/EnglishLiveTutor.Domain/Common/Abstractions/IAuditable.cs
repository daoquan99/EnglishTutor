namespace EnglishLiveTutor.Domain.Common.Abstractions;

/// <summary>Creation/update metadata. UTC getters retain compatibility with legacy CreatedAt/UpdatedAt columns.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAtUtc { get; }
    string? CreatedBy { get; }
    DateTimeOffset? UpdatedAtUtc { get; }
    string? UpdatedBy { get; }
}
