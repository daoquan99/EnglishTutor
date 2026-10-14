using EnglishLiveTutor.Domain.Common.Abstractions;
using EnglishLiveTutor.Domain.Conversations;
using EnglishLiveTutor.Domain.Learning;
using EnglishLiveTutor.Domain.Pronunciation;
using EnglishLiveTutor.Domain.Vocabulary;
using Xunit;

namespace EnglishLiveTutor.Domain.Tests;

public class DomainLogicTests
{
    [Fact]
    public void VocabularyRoot_InheritsAuditAndSoftDeletion()
    {
        var vocabulary = new VocabularyEntity();
        Assert.IsAssignableFrom<IAuditable>(vocabulary);
        Assert.IsAssignableFrom<ISoftDeletable>(vocabulary);
        Assert.IsAssignableFrom<IAuditable>(new VocabularySenseEntity());
        Assert.IsNotAssignableFrom<ISoftDeletable>(new VocabularySenseEntity());
        Assert.NotEqual(Guid.Empty, vocabulary.Id);
    }

    [Fact]
    public void ConversationSoftDeleteAndRestore_AreIdempotent()
    {
        var conversation = new ConversationEntity();
        var now = DateTimeOffset.UtcNow;
        conversation.SoftDelete(now, "tester");
        conversation.SoftDelete(now.AddDays(1), "someone-else");
        Assert.True(conversation.IsDeleted);
        Assert.Equal(now, conversation.DeletedAtUtc);
        Assert.Equal("tester", conversation.DeletedBy);
        conversation.Restore();
        conversation.Restore();
        Assert.False(conversation.IsDeleted);
        Assert.Null(conversation.DeletedAtUtc);
        Assert.Null(conversation.DeletedBy);
    }

    [Fact]
    public void NormalizeWord_ShouldLowercaseAndCollapseWhitespace()
    {
        string raw = "  Unprecedented   Word  ";
        string normalized = VocabularyEntity.NormalizeWord(raw);

        Assert.Equal("unprecedented word", normalized);
    }

    [Fact]
    public void ApplyReview_AgainRating_ShouldResetIntervalAndSetNextReviewIn4Hours()
    {
        var progress = new UserVocabularyProgressEntity
        {
            ReviewCount = 3,
            IntervalDays = 12,
            EaseFactor = 2.5m,
            MasteryLevel = 50,
            LearningStatus = "Review"
        };

        var now = DateTimeOffset.UtcNow;
        string result = progress.ApplyReview("Again", now);

        Assert.Equal("Again", result);
        Assert.Equal(4, progress.ReviewCount);
        Assert.Equal(1, progress.IncorrectCount);
        Assert.Equal(1, progress.IntervalDays);
        Assert.Equal(2.30m, progress.EaseFactor);
        Assert.Equal(35, progress.MasteryLevel);
        Assert.Equal("Learning", progress.LearningStatus);
        Assert.Equal(now.AddHours(4), progress.NextReviewAt);
    }

    [Fact]
    public void ApplyReview_EasyRating_AdvancesIntervalAndMastery()
    {
        var progress = new UserVocabularyProgressEntity
        {
            ReviewCount = 0,
            IntervalDays = 0,
            EaseFactor = 2.50m,
            MasteryLevel = 60,
            LearningStatus = "Learning"
        };

        var now = DateTimeOffset.UtcNow;
        string result = progress.ApplyReview("Easy", now);

        Assert.Equal("Easy", result);
        Assert.Equal(1, progress.ReviewCount);
        Assert.Equal(1, progress.CorrectCount);
        Assert.Equal(5, progress.IntervalDays);
        Assert.Equal(2.65m, progress.EaseFactor);
        Assert.Equal(90, progress.MasteryLevel);
        Assert.Equal("Mastered", progress.LearningStatus);
        Assert.Equal(now.AddDays(5), progress.NextReviewAt);
    }

    [Fact]
    public void ScoringRubric_ValidateWeights_ThrowsWhenTotalIsNot100()
    {
        var rubric = new ScoringRubricEntity
        {
            Name = "Test Rubric",
            Criteria = new List<ScoringRubricCriterionEntity>
            {
                new() { Code = "Accuracy", Weight = 50m },
                new() { Code = "Clarity", Weight = 40m } // Total 90m
            }
        };

        var ex = Assert.Throws<InvalidOperationException>(() => rubric.ValidateWeights());
        Assert.Contains("Tổng trọng số", ex.Message);
    }

    [Fact]
    public void ScoringRubric_ValidateWeights_PassesWhenTotalIs100()
    {
        var rubric = new ScoringRubricEntity
        {
            Name = "Valid Rubric",
            Criteria = new List<ScoringRubricCriterionEntity>
            {
                new() { Code = "Accuracy", Weight = 40m },
                new() { Code = "Clarity", Weight = 30m },
                new() { Code = "Fluency", Weight = 30m }
            }
        };

        rubric.ValidateWeights(); // Should not throw
    }

    [Fact]
    public void PronunciationAssessment_CalculateWeightedScore_RoundsAccurately()
    {
        decimal rawScore = 85.5m;
        decimal weight = 40.0m;

        decimal weighted = PronunciationAssessmentEntity.CalculateWeightedScore(rawScore, weight);

        // 85.5 * 40 / 100 = 34.20
        Assert.Equal(34.20m, weighted);
    }
}
