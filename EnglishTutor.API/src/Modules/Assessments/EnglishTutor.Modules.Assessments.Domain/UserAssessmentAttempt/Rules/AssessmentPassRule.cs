using EnglishTutor.BuildingBlocks.Domain.Rules;
using EnglishTutor.Modules.Assessments.Domain.Shared;

namespace EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Rules;

public sealed class AssessmentPassRule(
    int totalScore,
    IReadOnlyDictionary<AssessmentSkill, int> sectionScores,
    int passingScore,
    int minSkillScore) : IBusinessRule
{
    public bool IsBroken() =>
        totalScore < passingScore || sectionScores.Values.Any(score => score < minSkillScore);

    public string Message =>
        $"Assessment requires total score >= {passingScore} and every section score >= {minSkillScore}.";
}
