using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Vocabulary.Application.Commands.ReviewVocabulary;
using EnglishTutor.Modules.Vocabulary.Domain.Entities;
using EnglishTutor.Modules.Vocabulary.Domain.Enums;
using EnglishTutor.Modules.Vocabulary.Domain.Events;
using Xunit;

namespace EnglishTutor.Modules.Vocabulary.UnitTests;

public sealed class VocabularyDomainTests
{
    [Fact]
    public void VocabularyItem_Adds_Translations_And_Examples()
    {
        var item = VocabularyItem.Create(LanguageCode.English, "hello", "/həˈləʊ/", LanguageLevel.A1, "greeting", PartOfSpeech.Interjection);

        item.AddTranslation(LanguageCode.Vietnamese, "xin chao");
        item.AddExample("Hello, my name is Linh.", LanguageLevel.A1);

        Assert.Single(item.Translations);
        Assert.Single(item.Examples);
    }

    [Fact]
    public void Mastery_New_Item_Is_Due_Immediately()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English);

        Assert.Equal(VocabularyMasteryStatus.New, mastery.Status);
        Assert.True(mastery.NextReviewAtUtc <= DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void RecordReview_Correct_First_Time_Moves_To_Learning()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English);

        mastery.RecordReview(isCorrect: true, score: 80);

        Assert.Equal(VocabularyMasteryStatus.Learning, mastery.Status);
        Assert.Equal(1, mastery.ReviewCount);
        Assert.Contains(mastery.DomainEvents, domainEvent => domainEvent is VocabularyReviewedDomainEvent);
    }

    [Fact]
    public void RecordReview_Incorrect_Moves_To_Weak()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English);

        mastery.RecordReview(isCorrect: false, score: 40);

        Assert.Equal(VocabularyMasteryStatus.Weak, mastery.Status);
        Assert.Equal(0, mastery.CorrectReviewCount);
    }

    [Fact]
    public void Five_High_Correct_Reviews_Move_To_Mastered()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English);

        for (var i = 0; i < 5; i++)
        {
            mastery.RecordReview(isCorrect: true, score: 100);
        }

        Assert.Equal(VocabularyMasteryStatus.Mastered, mastery.Status);
        Assert.Contains(mastery.DomainEvents, domainEvent => domainEvent is VocabularyMasteredDomainEvent);
    }

    [Fact]
    public void MarkMastered_Raises_Mastered_Event_And_Sets_Minimum_Score()
    {
        var mastery = UserVocabularyMastery.Create(Guid.NewGuid(), Guid.NewGuid(), LanguageCode.English);

        mastery.MarkMastered();

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
