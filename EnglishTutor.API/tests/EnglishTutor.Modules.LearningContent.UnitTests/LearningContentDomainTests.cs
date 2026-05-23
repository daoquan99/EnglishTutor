using EnglishTutor.BuildingBlocks.Domain.Exceptions;
using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Enums;
using EnglishTutor.Modules.LearningContent.Domain.Lesson;
using EnglishTutor.Modules.LearningContent.Domain.Lesson.Events;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Enums;
using EnglishTutor.Modules.LearningContent.Domain.UserLearningPathCard.Events;
using Xunit;

namespace EnglishTutor.Modules.LearningContent.UnitTests;

public sealed class LearningContentDomainTests
{
    private static readonly DateTime UtcNow = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Lesson_Publish_Requires_At_Least_One_Section()
    {
        var lesson = CreateLesson();

        Assert.Throws<DomainException>(() => lesson.Publish(UtcNow));
    }

    [Fact]
    public void Lesson_Publish_Raises_Event()
    {
        var lesson = CreateLesson();
        lesson.AddSection("Intro", "Content", 1, "Theory", UtcNow);

        lesson.Publish(UtcNow);

        Assert.True(lesson.IsPublished);
        Assert.Contains(lesson.DomainEvents, item => item is LessonPublishedDomainEvent);
    }

    [Fact]
    public void ConversationScenario_Stores_Ordered_Lines()
    {
        var scenario = ConversationScenario.Create("en", LanguageLevel.A1, "Coffee", "Order coffee", "coffee shop", 1, 5, UtcNow);
        scenario.AddLine(ConversationSpeaker.User, 2, "Small, please.", null, null, null, UtcNow);
        scenario.AddLine(ConversationSpeaker.AI, 1, "What size?", null, null, null, UtcNow);

        Assert.Equal([1, 2], scenario.Lines.OrderBy(line => line.Order).Select(line => line.Order));
    }

    [Fact]
    public void LearningPathCard_MarkCompleted_Raises_LessonCompleted_Event()
    {
        var card = UserLearningPathCard.Create(
            Guid.NewGuid(),
            "en",
            ContentType.Lesson,
            Guid.NewGuid(),
            "Intro",
            "A1",
            "Speaking",
            "intro",
            LearningPathCardStatus.Available,
            1,
            UtcNow);

        card.MarkCompleted(120, UtcNow.AddMinutes(2));

        Assert.Equal(LearningPathCardStatus.Completed, card.Status);
        Assert.Contains(card.DomainEvents, item => item is LessonCompletedDomainEvent);
    }

    [Fact]
    public void Lesson_AddTranslation_Stores_Translation_Once_Per_Language()
    {
        var lesson = CreateLesson();

        lesson.AddTranslation("vi", "Tieu de", "Mo ta", UtcNow);
        lesson.AddTranslation("VI", "Khac", "Khac", UtcNow);

        Assert.Single(lesson.Translations);
    }

    [Fact]
    public void LessonSection_AddTranslation_Stores_Content()
    {
        var lesson = CreateLesson();
        var section = lesson.AddSection("Intro", "Content", 1, "Theory", UtcNow);

        section.AddTranslation("vi", "Mo dau", "Noi dung", UtcNow);

        Assert.Single(section.Translations);
    }

    [Fact]
    public void ConversationScenario_Publish_Requires_Line()
    {
        var scenario = ConversationScenario.Create("en", LanguageLevel.A1, "Cafe", "Order", "coffee shop", 1, 5, UtcNow);

        Assert.Throws<DomainException>(() => scenario.Publish(UtcNow));
    }

    [Fact]
    public void ConversationScenario_Publish_Succeeds_With_Line()
    {
        var scenario = ConversationScenario.Create("en", LanguageLevel.A1, "Cafe", "Order", "coffee shop", 1, 5, UtcNow);
        scenario.AddLine(ConversationSpeaker.AI, 1, "Hello", null, null, null, UtcNow);

        scenario.Publish(UtcNow);

        Assert.True(scenario.IsPublished);
    }

    [Fact]
    public void ConversationScenario_AddTranslation_Stores_Translation_Once()
    {
        var scenario = ConversationScenario.Create("en", LanguageLevel.A1, "Cafe", "Order", "coffee shop", 1, 5, UtcNow);

        scenario.AddTranslation("vi", "Quan ca phe", "Goi do", "quan", UtcNow);
        scenario.AddTranslation("VI", "Khac", "Khac", "khac", UtcNow);

        Assert.Single(scenario.Translations);
    }

    [Fact]
    public void ConversationLine_AddTranslation_Stores_Translation()
    {
        var scenario = ConversationScenario.Create("en", LanguageLevel.A1, "Cafe", "Order", "coffee shop", 1, 5, UtcNow);
        var line = scenario.AddLine(ConversationSpeaker.User, 1, "A coffee, please.", null, null, null, UtcNow);

        line.AddTranslation("vi", "Mot ca phe.", null, UtcNow);

        Assert.Single(line.Translations);
    }

    [Fact]
    public void LearningPathCard_Unlock_Changes_Locked_To_Available()
    {
        var card = UserLearningPathCard.Create(Guid.NewGuid(), "en", ContentType.Lesson, Guid.NewGuid(), "Intro", "A1", "Speaking", "intro", LearningPathCardStatus.Locked, 1, UtcNow);

        card.Unlock(UtcNow.AddMinutes(1));

        Assert.Equal(LearningPathCardStatus.Available, card.Status);
    }

    [Fact]
    public void ConversationPathCard_MarkCompleted_Raises_Conversation_Event()
    {
        var card = UserLearningPathCard.Create(Guid.NewGuid(), "en", ContentType.ConversationScenario, Guid.NewGuid(), "Cafe", "A1", "Conversation", "coffee", LearningPathCardStatus.Available, 1, UtcNow);

        card.MarkCompleted(90, UtcNow.AddMinutes(2));

        Assert.Contains(card.DomainEvents, item => item is ConversationScenarioCompletedDomainEvent);
    }

    private static Lesson CreateLesson() =>
        Lesson.Create("en", LanguageLevel.A1, "intro", LearningSkill.Speaking, "Intro", "Intro lesson", 1, 5, UtcNow);
}
