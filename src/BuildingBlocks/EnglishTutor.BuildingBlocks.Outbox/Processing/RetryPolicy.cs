namespace EnglishTutor.BuildingBlocks.Outbox.Processing;

public static class RetryPolicy
{
    public static DateTime CalculateNextRetry(int retryCount)
    {
        var delaySeconds = Math.Pow(2, retryCount) * 10;
        return DateTime.UtcNow.AddSeconds(delaySeconds);
    }
}
