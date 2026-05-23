using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Assessments.Infrastructure.Persistence.Repositories;

public sealed class AssessmentRubricRepository(AssessmentsDbContext dbContext) : IAssessmentRubricRepository
{
    public async Task<IReadOnlyList<AssessmentRubric>> ListByDefinitionAsync(Guid assessmentDefinitionId, CancellationToken cancellationToken) =>
        await dbContext.AssessmentRubrics
            .Where(rubric => rubric.AssessmentDefinitionId == assessmentDefinitionId)
            .OrderBy(rubric => rubric.Skill)
            .ToListAsync(cancellationToken);
}
