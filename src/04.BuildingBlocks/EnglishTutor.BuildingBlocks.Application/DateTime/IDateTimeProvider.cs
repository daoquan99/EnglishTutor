namespace EnglishTutor.BuildingBlocks.Application.DateTime;

public interface IDateTimeProvider
{
    System.DateTime UtcNow { get; }
}
