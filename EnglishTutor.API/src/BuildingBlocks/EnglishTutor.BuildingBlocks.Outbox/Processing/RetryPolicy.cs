namespace EnglishTutor.BuildingBlocks.Outbox.Processing;

public static class RetryPolicy
{
    public static DateTime CalculateNextRetry(int retryCount, DateTime utcNow)
    {
        var delaySeconds = Math.Min(Math.Pow(2, Math.Max(0, retryCount)) * 10, TimeSpan.FromHours(1).TotalSeconds);
        return utcNow.AddSeconds(delaySeconds);
    }
}
