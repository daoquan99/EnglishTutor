using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.BuildingBlocks.Infrastructure;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
