using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Commands.ReviewVocabulary;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;
using EnglishTutor.Modules.Vocabulary.Domain.Enums;
using EnglishTutor.Modules.Vocabulary.Domain.Events;
using EnglishTutor.Modules.Vocabulary.Infrastructure.Seed;
using Xunit;

namespace EnglishTutor.Modules.Vocabulary.UnitTests;

public sealed class VocabularyDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void VocabularyItem_Adds_Translations_And_Examples()
    {
        var item = VocabularyItem.Create(
            LanguageCode.English,
            "hello",
            "/hello/",
            LanguageLevel.A1,
            "greeting",
            PartOfSpeech.Interjection,
            UtcNow);

        item.AddTranslation(LanguageCode.Vietnamese, "xin chao");
        item.AddExample("Hello, my name is Linh.", LanguageLevel.A1);

        Assert.Single(item.Translations);
        Assert.Single(item.Examples);
    }

    [Fact]
    public void VocabularySeedData_Creates_Default_English_Items_With_Translations_And_Examples()
    {
        var items = VocabularySeedData.CreateDefaultEnglishItems(UtcNow);

        Assert.True(items.Count >= 20);
        Assert.All(items, item =>
        {
            Assert.Equal(LanguageCode.English, item.TargetLanguageCode);
            Assert.NotEmpty(item.Translations);
            Assert.NotEmpty(item.Examples);
            Assert.All(item.Examples, example => Assert.NotEmpty(example.Translations));
        });
    }

    [Fact]
    public void Mastery_New_Item_Is_Due_Immediately()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English, UtcNow);

        Assert.Equal(VocabularyMasteryStatus.New, mastery.Status);
        Assert.Equal(UtcNow, mastery.NextReviewAtUtc);
    }

    [Fact]
    public void RecordReview_Correct_First_Time_Moves_To_Learning()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English, UtcNow);

        mastery.RecordReview(isCorrect: true, score: 80, UtcNow);

        Assert.Equal(VocabularyMasteryStatus.Learning, mastery.Status);
        Assert.Equal(1, mastery.ReviewCount);
        Assert.Contains(mastery.DomainEvents, domainEvent => domainEvent is VocabularyReviewedDomainEvent);
    }

    [Fact]
    public void RecordReview_Incorrect_Moves_To_Weak()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English, UtcNow);

        mastery.RecordReview(isCorrect: false, score: 40, UtcNow);

        Assert.Equal(VocabularyMasteryStatus.Weak, mastery.Status);
        Assert.Equal(0, mastery.CorrectReviewCount);
    }

    [Fact]
    public void Five_High_Correct_Reviews_Move_To_Mastered()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English, UtcNow);

        for (var i = 0; i < 5; i++)
        {
            mastery.RecordReview(isCorrect: true, score: 100, UtcNow.AddDays(i));
        }

        Assert.Equal(VocabularyMasteryStatus.Mastered, mastery.Status);
        Assert.Contains(mastery.DomainEvents, domainEvent => domainEvent is VocabularyMasteredDomainEvent);
    }

    [Fact]
    public void MarkMastered_Raises_Mastered_Event_And_Sets_Minimum_Score()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English, UtcNow);

        // Satisfy CanMarkMastered: score >= 85, 3+ reviews, 2+ consecutive correct, status != New.
        for (var i = 0; i < 3; i++)
        {
            mastery.RecordReview(isCorrect: true, score: 90, UtcNow.AddDays(i));
        }

        mastery.MarkMastered(UtcNow);

        Assert.Equal(VocabularyMasteryStatus.Mastered, mastery.Status);
        Assert.True(mastery.MeaningMasteryScore >= 90);
    }

    [Fact]
    public void ReviewVocabularyValidator_Rejects_Out_Of_Range_Score()
    {
        var validator = new ReviewVocabularyCommandValidator();

        var result = validator.Validate(new ReviewVocabularyCommand(Guid.NewGuid(), Guid.NewGuid(), "en", true, 101));

        Assert.False(result.IsValid);
    }
}
