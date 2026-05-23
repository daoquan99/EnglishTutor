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

public sealed class AssessmentAttemptRepository(AssessmentsDbContext dbContext) : IAssessmentAttemptRepository
{
    public Task<UserAssessmentAttempt?> GetByIdWithAnswersAsync(Guid attemptId, CancellationToken cancellationToken) =>
        dbContext.UserAssessmentAttempts
            .Include(attempt => attempt.Answers)
            .SingleOrDefaultAsync(attempt => attempt.Id == attemptId, cancellationToken);

    public Task<AssessmentGradingResult?> GetResultByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken) =>
        dbContext.AssessmentGradingResults
            .SingleOrDefaultAsync(result => result.AttemptId == attemptId, cancellationToken);

    public Task<bool> HasRecentFailedAttemptAsync(
        Guid userId,
        string targetLanguageCode,
        string currentLevel,
        DateTime sinceUtc,
        CancellationToken cancellationToken)
    {
        var normalizedLanguage = targetLanguageCode.Trim().ToLowerInvariant();
        return dbContext.UserAssessmentAttempts.AnyAsync(
            attempt =>
                attempt.UserId == userId &&
                attempt.TargetLanguageCode == normalizedLanguage &&
                attempt.CurrentLevel == currentLevel &&
                attempt.Status == AssessmentAttemptStatus.Failed &&
                attempt.GradedAtUtc >= sinceUtc,
            cancellationToken);
    }

    public async Task AddAsync(UserAssessmentAttempt attempt, CancellationToken cancellationToken) =>
        await dbContext.UserAssessmentAttempts.AddAsync(attempt, cancellationToken);

    public async Task AddAnswerAsync(UserAssessmentAnswer answer, CancellationToken cancellationToken) =>
        await dbContext.UserAssessmentAnswers.AddAsync(answer, cancellationToken);

    public async Task AddResultAsync(AssessmentGradingResult result, CancellationToken cancellationToken) =>
        await dbContext.AssessmentGradingResults.AddAsync(result, cancellationToken);
}
