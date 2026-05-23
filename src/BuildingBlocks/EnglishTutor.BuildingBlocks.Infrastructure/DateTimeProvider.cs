using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.BuildingBlocks.Infrastructure;

public sealed class DateTimeProvider(TimeProvider timeProvider) : IDateTimeProvider
{
    public DateTime UtcNow => timeProvider.GetUtcNow().UtcDateTime;
}
