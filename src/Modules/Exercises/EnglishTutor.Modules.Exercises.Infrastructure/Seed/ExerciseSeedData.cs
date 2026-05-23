using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet.Enums;
using EnglishTutor.Modules.Exercises.Domain.Shared;
using EnglishTutor.Modules.Exercises.Domain.UserExerciseAttempt.Enums;
using EnglishTutor.Modules.Exercises.Domain.ExerciseSet;

namespace EnglishTutor.Modules.Exercises.Infrastructure.Seed;

internal static class ExerciseSeedData
{
    public static IReadOnlyList<ExerciseSet> CreateDefaultSets(DateTime utcNow)
    {
        var sets = new List<ExerciseSet>
        {
            CreateMultipleChoice("A1 daily vocabulary choice", "daily life", utcNow, [
                ("Choose the greeting.", "Hello", new[] { "Hello", "Blue", "Chair", "Run" }),
                ("Choose the food word.", "Apple", new[] { "Table", "Apple", "Window", "Book" }),
                ("Choose the place word.", "School", new[] { "Fast", "School", "Happy", "Drink" }),
                ("Choose the color.", "Green", new[] { "Green", "Walk", "Water", "Name" }),
                ("Choose the family word.", "Mother", new[] { "Mother", "Pencil", "Sleep", "Cold" })
            ]),
            CreateMultipleChoice("A1 basic grammar choice", "grammar", utcNow, [
                ("I ___ a student.", "am", new[] { "am", "is", "are", "be" }),
                ("She ___ from Vietnam.", "is", new[] { "am", "is", "are", "do" }),
                ("They ___ happy.", "are", new[] { "is", "am", "are", "does" }),
                ("This is ___ book.", "my", new[] { "I", "me", "my", "mine" }),
                ("We ___ English every day.", "study", new[] { "study", "studies", "studying", "studied" })
            ]),
            CreateFillInTheBlank("A1 fill in simple sentences", "daily life", utcNow, [
                ("My name ___ Nam.", "is", "Use the verb be."),
                ("I live ___ Hanoi.", "in", "Use a place preposition."),
                ("She has ___ apple.", "an", "Use the article before a vowel sound."),
                ("We go to school ___ bus.", "by", "Use by for transportation."),
                ("He is ___ teacher.", "a", "Use the indefinite article.")
            ]),
            CreateFillInTheBlank("A1 fill in questions", "questions", utcNow, [
                ("___ are you?", "how", "Ask about condition."),
                ("___ is your name?", "what", "Ask for information."),
                ("___ do you live?", "where", "Ask about place."),
                ("___ old are you?", "how", "Ask about age."),
                ("___ is she?", "who", "Ask about a person.")
            ]),
            CreateSimpleSet(ExerciseType.VerbConjugation, LearningSkill.Grammar, "A1 verb conjugation", "verbs", utcNow, [
                ("I ___ coffee every morning. (drink)", "drink", "Use base verb with I."),
                ("She ___ English at night. (study)", "studies", "Use -ies with she."),
                ("They ___ soccer on Sundays. (play)", "play", "Use base verb with they."),
                ("He ___ to work by bus. (go)", "goes", "Use goes with he."),
                ("We ___ dinner at 7 PM. (eat)", "eat", "Use base verb with we.")
            ]),
            CreateSimpleSet(ExerciseType.SentenceCorrection, LearningSkill.Grammar, "A1 sentence correction", "grammar", utcNow, [
                ("Correct: She are my friend.", "She is my friend.", "Use is with she."),
                ("Correct: I has a book.", "I have a book.", "Use have with I."),
                ("Correct: They is students.", "They are students.", "Use are with they."),
                ("Correct: He go to school.", "He goes to school.", "Use goes with he."),
                ("Correct: This are my pen.", "This is my pen.", "Use is with this.")
            ]),
            CreateSimpleSet(ExerciseType.SentenceOrdering, LearningSkill.Grammar, "A1 sentence ordering", "word order", utcNow, [
                ("Order: name / my / is / Linh", "My name is Linh.", "Use subject before verb."),
                ("Order: am / I / a / developer", "I am a developer.", "Use I am."),
                ("Order: English / study / every day / I", "I study English every day.", "Use subject, verb, object, time.")
            ])
        };

        return sets;
    }

    private static ExerciseSet CreateMultipleChoice(
        string title,
        string topic,
        DateTime utcNow,
        IReadOnlyList<(string Prompt, string CorrectAnswer, IReadOnlyList<string> Options)> questions)
    {
        var set = ExerciseSet.Create("en", LanguageLevel.A1, topic, LearningSkill.Vocabulary, ExerciseType.MultipleChoice, title, null, utcNow);

        for (var index = 0; index < questions.Count; index++)
        {
            var questionData = questions[index];
            var question = set.AddQuestion(
                ExerciseType.MultipleChoice,
                questionData.Prompt,
                questionData.CorrectAnswer,
                $"Correct answer: {questionData.CorrectAnswer}.",
                index + 1,
                QuestionDifficulty.Easy,
                false,
                utcNow);

            for (var optionIndex = 0; optionIndex < questionData.Options.Count; optionIndex++)
            {
                var option = questionData.Options[optionIndex];
                question.AddOption(option, string.Equals(option, questionData.CorrectAnswer, StringComparison.OrdinalIgnoreCase), optionIndex + 1, utcNow);
            }
        }

        set.Publish(utcNow);
        return set;
    }

    private static ExerciseSet CreateFillInTheBlank(
        string title,
        string topic,
        DateTime utcNow,
        IReadOnlyList<(string Prompt, string CorrectAnswer, string Explanation)> questions) =>
        CreateSimpleSet(ExerciseType.FillInTheBlank, LearningSkill.Grammar, title, topic, utcNow, questions);

    private static ExerciseSet CreateSimpleSet(
        ExerciseType type,
        LearningSkill skill,
        string title,
        string topic,
        DateTime utcNow,
        IReadOnlyList<(string Prompt, string CorrectAnswer, string Explanation)> questions)
    {
        var set = ExerciseSet.Create("en", LanguageLevel.A1, topic, skill, type, title, null, utcNow);

        for (var index = 0; index < questions.Count; index++)
        {
            var question = questions[index];
            set.AddQuestion(
                type,
                question.Prompt,
                question.CorrectAnswer,
                question.Explanation,
                index + 1,
                QuestionDifficulty.Easy,
                false,
                utcNow);
        }

        set.Publish(utcNow);
        return set;
    }
}
