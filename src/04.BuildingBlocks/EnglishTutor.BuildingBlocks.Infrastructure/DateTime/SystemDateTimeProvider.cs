using EnglishTutor.BuildingBlocks.Application.DateTime;

namespace EnglishTutor.BuildingBlocks.Infrastructure.DateTime;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public System.DateTime UtcNow => System.DateTime.UtcNow;
}
