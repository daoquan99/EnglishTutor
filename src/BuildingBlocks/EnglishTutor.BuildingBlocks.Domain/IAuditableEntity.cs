namespace EnglishTutor.BuildingBlocks.Domain;

public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; }
    Guid? CreatedByUserId { get; }
    DateTime UpdatedAtUtc { get; }
    Guid? UpdatedByUserId { get; }
}
