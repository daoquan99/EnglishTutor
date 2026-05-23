using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario;
using EnglishTutor.Modules.LearningContent.Domain.ConversationScenario.Enums;
using EnglishTutor.Modules.LearningContent.Domain.Lesson;
using EnglishTutor.Modules.LearningContent.Domain.SentencePattern;

namespace EnglishTutor.Modules.LearningContent.Infrastructure.Seed;

public static class LearningContentSeedData
{
    public static IReadOnlyList<Lesson> CreateLessons(DateTime utcNow)
    {
        var lessons = new List<Lesson>();
        lessons.Add(CreateLesson("Daily greetings", "Simple greetings for everyday conversations.", "greetings", LearningSkill.Speaking, 1, utcNow));
        lessons.Add(CreateLesson("Introduce yourself", "Say your name, job, and learning goal.", "introduction", LearningSkill.Speaking, 2, utcNow));
        lessons.Add(CreateLesson("Present simple basics", "Use present simple for habits and facts.", "grammar", LearningSkill.Grammar, 3, utcNow));
        lessons.Add(CreateLesson("Useful study phrases", "Phrases for asking questions during learning.", "study", LearningSkill.Vocabulary, 4, utcNow));
        lessons.Add(CreateLesson("Ordering coffee", "Practice polite requests in a cafe.", "travel", LearningSkill.Conversation, 5, utcNow));
        return lessons;
    }

    public static IReadOnlyList<ConversationScenario> CreateScenarios(DateTime utcNow)
    {
        var coffee = ConversationScenario.Create("en", LanguageLevel.A1, "Coffee shop", "Order a drink politely.", "coffee shop", 1, 8, utcNow);
        coffee.AddTranslation("vi", "Quan ca phe", "Goi do uong mot cach lich su.", "quan ca phe", utcNow);
        coffee.AddLine(ConversationSpeaker.AI, 1, "Hello, what would you like to drink?", null, null, null, utcNow);
        coffee.AddLine(ConversationSpeaker.User, 2, "I would like a coffee, please.", "Ask for one drink politely.", null, null, utcNow);
        coffee.AddLine(ConversationSpeaker.AI, 3, "Sure. Small or large?", null, null, null, utcNow);
        coffee.AddLine(ConversationSpeaker.User, 4, "Small, please.", "Choose a size.", null, null, utcNow);
        coffee.Publish(utcNow);

        var intro = ConversationScenario.Create("en", LanguageLevel.A1, "Introducing yourself", "Share your name and work.", "first meeting", 1, 7, utcNow);
        intro.AddTranslation("vi", "Gioi thieu ban than", "Chia se ten va cong viec.", "gap lan dau", utcNow);
        intro.AddLine(ConversationSpeaker.AI, 1, "Hi, what is your name?", null, null, null, utcNow);
        intro.AddLine(ConversationSpeaker.User, 2, "My name is Linh.", "Say your name.", null, null, utcNow);
        intro.AddLine(ConversationSpeaker.AI, 3, "Nice to meet you. What do you do?", null, null, null, utcNow);
        intro.AddLine(ConversationSpeaker.User, 4, "I am a C# developer.", "Say your job.", null, null, utcNow);
        intro.Publish(utcNow);

        var directions = ConversationScenario.Create("en", LanguageLevel.A1, "Asking directions", "Ask where a place is.", "street", 2, 10, utcNow);
        directions.AddTranslation("vi", "Hoi duong", "Hoi vi tri mot dia diem.", "duong pho", utcNow);
        directions.AddLine(ConversationSpeaker.User, 1, "Excuse me, where is the station?", "Ask for a location.", null, null, utcNow);
        directions.AddLine(ConversationSpeaker.AI, 2, "Go straight and turn left.", null, null, null, utcNow);
        directions.AddLine(ConversationSpeaker.User, 3, "Thank you very much.", "Say thanks.", null, null, utcNow);
        directions.Publish(utcNow);

        return [coffee, intro, directions];
    }

    public static IReadOnlyList<SentencePattern> CreateSentencePatterns(DateTime utcNow) =>
    [
        SentencePattern.Create("en", LanguageLevel.A1, "Subject + verb + object", "Use this for simple actions.", "[\"I study English.\",\"She drinks coffee.\"]", "grammar", utcNow),
        SentencePattern.Create("en", LanguageLevel.A1, "Subject + am/is/are + adjective", "Use this to describe people or things.", "[\"I am ready.\",\"The lesson is easy.\"]", "grammar", utcNow),
        SentencePattern.Create("en", LanguageLevel.A2, "Subject + have/has + past participle", "Use this for present perfect.", "[\"I have finished.\",\"She has practiced.\"]", "grammar", utcNow)
    ];

    private static Lesson CreateLesson(string title, string description, string topic, LearningSkill skill, int order, DateTime utcNow)
    {
        var lesson = Lesson.Create("en", LanguageLevel.A1, topic, skill, title, description, order, 10, utcNow);
        lesson.AddTranslation("vi", title, description, utcNow);
        lesson.AddSection("Core idea", $"# {title}\n\n{description}", 1, "Theory", utcNow);
        lesson.AddSection("Practice", "Say two examples aloud and compare with the model.", 2, "Practice", utcNow);
        lesson.Publish(utcNow);
        return lesson;
    }
}
