using EnglishTutor.BuildingBlocks.SharedKernel;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Enums;
using EnglishTutor.Modules.Assessments.Domain.Shared;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Assessments.Infrastructure.Persistence.Repositories;

public sealed class AssessmentDefinitionRepository(AssessmentsDbContext dbContext) : IAssessmentDefinitionRepository
{
    public async Task<IReadOnlyList<AssessmentDefinition>> ListAvailableAsync(
        Guid userId,
        string targetLanguageCode,
        string currentLevel,
        CancellationToken cancellationToken)
    {
        _ = userId;

        var normalizedLanguage = targetLanguageCode.Trim().ToLowerInvariant();
        if (!Enum.TryParse<LanguageLevel>(currentLevel, true, out var level))
        {
            return [];
        }

        return await dbContext.AssessmentDefinitions
            .Where(definition =>
                definition.IsActive &&
                definition.AssessmentType == AssessmentType.LevelUpTest &&
                definition.TargetLanguageCode == normalizedLanguage &&
                definition.ForLevel == level)
            .OrderBy(definition => definition.Title)
            .ToListAsync(cancellationToken);
    }

    public Task<AssessmentDefinition?> GetByIdWithDetailsAsync(Guid definitionId, CancellationToken cancellationToken) =>
        dbContext.AssessmentDefinitions
            .Include(definition => definition.Sections)
                .ThenInclude(section => section.Questions)
            .Include(definition => definition.Rubrics)
            .SingleOrDefaultAsync(definition => definition.Id == definitionId, cancellationToken);

    public Task<AssessmentDefinition?> GetActiveLevelUpAsync(string targetLanguageCode, string currentLevel, CancellationToken cancellationToken)
    {
        var normalizedLanguage = targetLanguageCode.Trim().ToLowerInvariant();
        if (!Enum.TryParse<LanguageLevel>(currentLevel, true, out var level))
        {
            return Task.FromResult<AssessmentDefinition?>(null);
        }

        return dbContext.AssessmentDefinitions
            .Include(definition => definition.Sections)
                .ThenInclude(section => section.Questions)
            .Include(definition => definition.Rubrics)
            .Where(definition =>
                definition.IsActive &&
                definition.AssessmentType == AssessmentType.LevelUpTest &&
                definition.TargetLanguageCode == normalizedLanguage &&
                definition.ForLevel == level)
            .OrderBy(definition => definition.Title)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(AssessmentDefinition definition, CancellationToken cancellationToken) =>
        await dbContext.AssessmentDefinitions.AddAsync(definition, cancellationToken);
}
