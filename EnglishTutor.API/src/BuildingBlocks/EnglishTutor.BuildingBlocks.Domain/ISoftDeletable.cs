namespace EnglishTutor.BuildingBlocks.Domain;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAtUtc { get; }
    Guid? DeletedByUserId { get; }
}
