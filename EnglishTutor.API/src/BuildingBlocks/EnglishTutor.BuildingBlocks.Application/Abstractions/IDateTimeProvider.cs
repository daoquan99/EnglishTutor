namespace EnglishTutor.BuildingBlocks.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
