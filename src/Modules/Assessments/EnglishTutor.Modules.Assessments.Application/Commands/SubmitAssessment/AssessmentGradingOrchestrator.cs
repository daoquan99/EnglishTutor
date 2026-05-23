using System.Text.Json;
using EnglishTutor.Modules.AI.Contracts.DTOs;
using EnglishTutor.Modules.AI.Contracts.Services;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Enums;
using EnglishTutor.Modules.Assessments.Domain.Shared;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Enums;

namespace EnglishTutor.Modules.Assessments.Application.Commands.SubmitAssessment;

public sealed class AssessmentGradingOrchestrator(IAssessmentGradingService gradingService)
{
    public async Task<AssessmentGradingOutcome> GradeAsync(
        AssessmentDefinition definition,
        UserAssessmentAttempt attempt,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var sectionWeightTotal = definition.Sections.Sum(section => section.Weight);
        if (Math.Abs(sectionWeightTotal - 1m) > 0.0001m)
        {
            throw new InvalidOperationException("Assessment section weights must total 1.0.");
        }

        var sectionScores = new Dictionary<AssessmentSkill, int>();

        foreach (var section in definition.Sections)
        {
            var scored = 0m;
            var max = 0m;

            foreach (var question in section.Questions)
            {
                var answer = attempt.Answers.SingleOrDefault(item => item.QuestionId == question.Id)
                    ?? throw new InvalidOperationException("Assessment answer is missing.");
                var score = question.IsAiGraded
                    ? await GradeWithAiAsync(definition, section, question, answer, attempt.UserId, cancellationToken)
                    : GradeStatic(question, answer.UserAnswer);

                answer.ApplyScore(score.Score, score.Feedback, utcNow);
                scored += score.Score;
                max += question.MaxScore;
            }

            sectionScores[section.Skill] = max == 0
                ? 0
                : (int)Math.Round(scored / max * 100m);
        }

        var weightedTotal = definition.Sections.Sum(section =>
            sectionScores.GetValueOrDefault(section.Skill) * section.Weight);
        var totalScore = (int)Math.Round(weightedTotal);
        var weakSkills = sectionScores
            .Where(pair => pair.Value < definition.MinSkillScore)
            .Select(pair => pair.Key.ToString())
            .ToArray();
        var notes = JsonSerializer.Serialize(sectionScores.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value));

        return new AssessmentGradingOutcome(sectionScores, Math.Clamp(totalScore, 0, 100), weakSkills, notes);
    }

    private async Task<AssessmentQuestionScore> GradeWithAiAsync(
        AssessmentDefinition definition,
        AssessmentSection section,
        AssessmentQuestion question,
        UserAssessmentAnswer answer,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var rubric = definition.Rubrics.FirstOrDefault(item => item.Skill == section.Skill);
        var grading = await gradingService.GradeAsync(
            new GradingRequest(
                userId,
                definition.TargetLanguageCode,
                definition.ForLevel.ToString(),
                question.Prompt,
                answer.UserAnswer,
                rubric?.ScoringGuide ?? $"Score this {section.Skill} answer from 0 to {question.MaxScore}."),
            cancellationToken);

        var scaled = (int)Math.Round(Math.Clamp(grading.Score, 0, 100) * question.MaxScore / 100m);
        return new AssessmentQuestionScore(scaled, grading.Feedback);
    }

    private static AssessmentQuestionScore GradeStatic(AssessmentQuestion question, string userAnswer)
    {
        var normalizedUserAnswer = Normalize(userAnswer);
        var normalizedCorrectAnswer = Normalize(question.CorrectAnswer ?? string.Empty);
        var isCorrect = normalizedUserAnswer == normalizedCorrectAnswer;
        return new AssessmentQuestionScore(isCorrect ? question.MaxScore : 0, isCorrect ? "Correct." : "Incorrect.");
    }

    private static string Normalize(string value) =>
        string.Join(' ', value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
}

public sealed record AssessmentQuestionScore(int Score, string Feedback);

public sealed record AssessmentGradingOutcome(
    IReadOnlyDictionary<AssessmentSkill, int> SectionScores,
    int TotalScore,
    IReadOnlyList<string> WeakSkills,
    string SectionScoresJson);
