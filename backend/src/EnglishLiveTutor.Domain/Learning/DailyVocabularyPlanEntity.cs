using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Learning;

public sealed class DailyVocabularyPlanEntity : AggregateRoot<Guid>
{
    public DailyVocabularyPlanEntity() { Id = Guid.NewGuid(); }

    public string UserId { get; set; } = "default-learner";
    public string LearningDate { get; set; } = string.Empty; // yyyy-MM-dd in User TimeZone
    public string TimeZoneId { get; set; } = "Asia/Ho_Chi_Minh";
    public int NewVocabularyTarget { get; set; } = 20;
    public string Status { get; set; } = "Active"; // Active | Completed

    public DateTimeOffset? CompletedAt { get; set; }

    public List<DailyVocabularyItemEntity> Items { get; set; } = new();

    public void Complete(DateTimeOffset now)
    {
        Status = "Completed";
        CompletedAt = now;
    }

    public static string ResolveLocalLearningDate(string? timeZoneId)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(timeZoneId))
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
                return localNow.ToString("yyyy-MM-dd");
            }
        }
        catch
        {
            // Fallback to UTC+7 (Asia/Ho_Chi_Minh)
        }
        return DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("yyyy-MM-dd");
    }
}
