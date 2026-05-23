using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Enums;
using EnglishTutor.Modules.Assessments.Domain.Shared;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Enums;

namespace EnglishTutor.Modules.Assessments.Infrastructure.Seed;

internal static class AssessmentSeedData
{
    public static IReadOnlyList<AssessmentDefinition> CreateDefaultAssessments(DateTime utcNow)
    {
        var levels = new[] { LanguageLevel.A1, LanguageLevel.A2, LanguageLevel.B1, LanguageLevel.B2, LanguageLevel.C1 };
        return levels.Select(level => CreateEnglishLevelUpAssessment(level, utcNow)).ToArray();
    }

    private static AssessmentDefinition CreateEnglishLevelUpAssessment(LanguageLevel level, DateTime utcNow)
    {
        var definition = AssessmentDefinition.Create(
            AssessmentType.LevelUpTest,
            "en",
            level,
            $"English {level} level-up assessment",
            $"Placement-style level-up assessment for learners currently at {level}.",
            passingScore: 70,
            minSkillScore: 60,
            timeLimitMinutes: 45,
            utcNow);

        var grammar = definition.AddSection(AssessmentSkill.Grammar, "Grammar", 0.25m, 1, utcNow);
        grammar.AddQuestion(
            "Choose the correct sentence: I ____ coffee every morning.",
            "fill_in_blank",
            "drink",
            isAiGraded: false,
            maxScore: 10,
            order: 1,
            "The present simple form for I is drink.",
            utcNow);

        var vocabulary = definition.AddSection(AssessmentSkill.Vocabulary, "Vocabulary", 0.25m, 2, utcNow);
        vocabulary.AddQuestion(
            "Which word means 'to improve gradually through practice'?",
            "short_answer",
            "develop",
            isAiGraded: false,
            maxScore: 10,
            order: 1,
            "Develop means grow or improve over time.",
            utcNow);

        var writing = definition.AddSection(AssessmentSkill.Writing, "Writing", 0.25m, 3, utcNow);
        writing.AddQuestion(
            "Write 4-6 sentences about how you study English and what you want to improve next.",
            "short_writing",
            null,
            isAiGraded: true,
            maxScore: 20,
            order: 1,
            "AI grades clarity, grammar, vocabulary range, and coherence.",
            utcNow);

        var speaking = definition.AddSection(AssessmentSkill.Speaking, "Speaking", 0.25m, 4, utcNow);
        speaking.AddQuestion(
            "Record or write a short answer: introduce yourself and describe your learning goal.",
            "spoken_response",
            null,
            isAiGraded: true,
            maxScore: 20,
            order: 1,
            "AI grades relevance, pronunciation proxy, fluency, and structure.",
            utcNow);

        definition.AddRubric(
            AssessmentSkill.Writing,
            "Writing response must be relevant, coherent, grammatically controlled, and use appropriate vocabulary.",
            20,
            "Return JSON with score 0-100 and feedback. Penalize off-topic, very short, or untranslated responses.",
            utcNow);
        definition.AddRubric(
            AssessmentSkill.Speaking,
            "Speaking response must answer the prompt clearly and match the learner level.",
            20,
            "Return JSON with score 0-100 and feedback. Grade the provided transcript as a speaking proxy until STT scoring is enabled.",
            utcNow);

        return definition;
    }
}
