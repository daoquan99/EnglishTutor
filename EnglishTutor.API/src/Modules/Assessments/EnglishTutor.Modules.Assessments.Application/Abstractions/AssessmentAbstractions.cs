using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Enums;
using EnglishTutor.Modules.Assessments.Domain.Shared;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Enums;

namespace EnglishTutor.Modules.Assessments.Application.Abstractions;

public interface IAssessmentDefinitionRepository
{
    Task<IReadOnlyList<AssessmentDefinition>> ListAvailableAsync(Guid userId, string targetLanguageCode, string currentLevel, CancellationToken cancellationToken);
    Task<AssessmentDefinition?> GetByIdWithDetailsAsync(Guid definitionId, CancellationToken cancellationToken);
    Task<AssessmentDefinition?> GetActiveLevelUpAsync(string targetLanguageCode, string currentLevel, CancellationToken cancellationToken);
    Task AddAsync(AssessmentDefinition definition, CancellationToken cancellationToken);
}

public interface IAssessmentAttemptRepository
{
    Task<UserAssessmentAttempt?> GetByIdWithAnswersAsync(Guid attemptId, CancellationToken cancellationToken);
    Task<AssessmentGradingResult?> GetResultByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken);
    Task<bool> HasRecentFailedAttemptAsync(Guid userId, string targetLanguageCode, string currentLevel, DateTime sinceUtc, CancellationToken cancellationToken);
    Task AddAsync(UserAssessmentAttempt attempt, CancellationToken cancellationToken);
    Task AddAnswerAsync(UserAssessmentAnswer answer, CancellationToken cancellationToken);
    Task AddResultAsync(AssessmentGradingResult result, CancellationToken cancellationToken);
}

public interface IAssessmentRubricRepository
{
    Task<IReadOnlyList<AssessmentRubric>> ListByDefinitionAsync(Guid assessmentDefinitionId, CancellationToken cancellationToken);
}

public interface IAssessmentsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
