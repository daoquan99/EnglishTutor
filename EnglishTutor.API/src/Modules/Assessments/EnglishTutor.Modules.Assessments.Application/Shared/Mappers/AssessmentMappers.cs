using EnglishTutor.Modules.Assessments.Application.Queries.GetAvailableAssessments;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;

namespace EnglishTutor.Modules.Assessments.Application.Shared.Mappers;

public static class AssessmentMappers
{
    public static AvailableAssessmentResponse ToAvailableResponse(this AssessmentDefinition definition) =>
        new(
            definition.Id,
            definition.AssessmentType.ToString(),
            definition.TargetLanguageCode,
            definition.ForLevel.ToString(),
            definition.Title,
            definition.Description,
            definition.PassingScore,
            definition.MinSkillScore,
            definition.TimeLimitMinutes);

    public static AttemptDetailResponse ToDetailResponse(this UserAssessmentAttempt attempt, AssessmentDefinition definition) =>
        new(
            attempt.Id,
            definition.Id,
            attempt.TargetLanguageCode,
            attempt.CurrentLevel,
            attempt.Status.ToString(),
            definition.Sections
                .OrderBy(section => section.Order)
                .Select(section => new AssessmentSectionResponse(
                    section.Id,
                    section.Skill.ToString(),
                    section.Title,
                    section.Weight,
                    section.Order,
                    section.Questions
                        .OrderBy(question => question.Order)
                        .Select(question => new AssessmentQuestionResponse(
                            question.Id,
                            question.Prompt,
                            question.QuestionType,
                            question.IsAiGraded,
                            question.MaxScore,
                            question.Order,
                            question.Explanation))
                        .ToArray()))
                .ToArray());
}
