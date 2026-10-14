using EnglishLiveTutor.Domain.Common.Primitives;

namespace EnglishLiveTutor.Domain.Learning;

public sealed class UserVocabularyProgressEntity : AggregateRoot<Guid>
{
    public UserVocabularyProgressEntity() { Id = Guid.NewGuid(); }

    public string UserId { get; set; } = "default-learner";
    public Guid VocabularyId { get; set; }
    public Guid SenseId { get; set; }
    public int MasteryLevel { get; set; } // 0..100
    public int CorrectCount { get; set; }
    public int IncorrectCount { get; set; }
    public int ReviewCount { get; set; }
    public DateTimeOffset? LastReviewedAt { get; set; }
    public DateTimeOffset? NextReviewAt { get; set; }
    public int IntervalDays { get; set; }
    public decimal EaseFactor { get; set; } = 2.50m;
    public string LearningStatus { get; set; } = "New"; // New | Learning | Review | Mastered
    public decimal? LatestPronunciationScore { get; set; }

    public string ApplyReview(string? ratingRaw, DateTimeOffset now)
    {
        string rating = (ratingRaw ?? "Good").Trim();
        ReviewCount += 1;
        LastReviewedAt = now;

        switch (rating.ToLowerInvariant())
        {
            case "again":
                rating = "Again";
                IncorrectCount += 1;
                IntervalDays = 1;
                EaseFactor = Math.Max(1.30m, EaseFactor - 0.20m);
                MasteryLevel = Math.Max(0, MasteryLevel - 15);
                LearningStatus = "Learning";
                NextReviewAt = now.AddHours(4);
                break;

            case "hard":
                rating = "Hard";
                CorrectCount += 1;
                IntervalDays = Math.Max(1, (int)Math.Round(Math.Max(1, IntervalDays) * 1.2m));
                EaseFactor = Math.Max(1.30m, EaseFactor - 0.15m);
                MasteryLevel = Math.Min(100, MasteryLevel + 12);
                LearningStatus = "Review";
                NextReviewAt = now.AddDays(IntervalDays);
                break;

            case "easy":
                rating = "Easy";
                CorrectCount += 1;
                IntervalDays = IntervalDays <= 1
                    ? 5
                    : (int)Math.Round(IntervalDays * EaseFactor * 1.3m);
                EaseFactor = Math.Min(3.00m, EaseFactor + 0.15m);
                MasteryLevel = Math.Min(100, MasteryLevel + 30);
                LearningStatus = MasteryLevel >= 80 ? "Mastered" : "Review";
                NextReviewAt = now.AddDays(IntervalDays);
                break;

            default:
                rating = "Good";
                CorrectCount += 1;
                IntervalDays = IntervalDays <= 1
                    ? 3
                    : (int)Math.Round(IntervalDays * EaseFactor);
                MasteryLevel = Math.Min(100, MasteryLevel + 22);
                LearningStatus = MasteryLevel >= 85 ? "Mastered" : "Review";
                NextReviewAt = now.AddDays(IntervalDays);
                break;
        }

        return rating;
    }
}
