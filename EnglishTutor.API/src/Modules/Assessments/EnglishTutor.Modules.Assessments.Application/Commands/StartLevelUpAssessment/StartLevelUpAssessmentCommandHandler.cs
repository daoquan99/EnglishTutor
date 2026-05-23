using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;
using EnglishTutor.Modules.Assessments.Application.Shared.Errors;
using EnglishTutor.Modules.Assessments.Application.Shared.Mappers;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition;
using EnglishTutor.Modules.Assessments.Domain.AssessmentDefinition.Entities;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt;
using EnglishTutor.Modules.Assessments.Domain.UserAssessmentAttempt.Entities;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Assessments.Application.Commands.StartLevelUpAssessment;

public sealed class StartLevelUpAssessmentCommandHandler(
    IAssessmentDefinitionRepository definitionRepository,
    IAssessmentAttemptRepository attemptRepository,
    IUserTargetLanguageReader targetLanguageReader,
    IAssessmentsUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<StartLevelUpAssessmentCommand, AttemptDetailResponse>
{
    public async Task<Result<AttemptDetailResponse>> Handle(StartLevelUpAssessmentCommand request, CancellationToken cancellationToken)
    {
        var targetLanguages = await targetLanguageReader.GetByUserIdAsync(request.UserId, cancellationToken);
        var targetLanguage = targetLanguages.FirstOrDefault(language =>
                language.IsActive &&
                (string.IsNullOrWhiteSpace(request.TargetLanguageCode) ||
                    language.TargetLanguageCode.Equals(request.TargetLanguageCode.Trim(), StringComparison.OrdinalIgnoreCase)))
            ?? targetLanguages.FirstOrDefault(language =>
                string.IsNullOrWhiteSpace(request.TargetLanguageCode) ||
                language.TargetLanguageCode.Equals(request.TargetLanguageCode.Trim(), StringComparison.OrdinalIgnoreCase));
        if (targetLanguage is null)
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.TargetLanguageNotFound);
        }

        var utcNow = dateTimeProvider.UtcNow;
        if (await attemptRepository.HasRecentFailedAttemptAsync(
                request.UserId,
                targetLanguage.TargetLanguageCode,
                targetLanguage.CurrentLevel,
                utcNow.AddDays(-7),
                cancellationToken))
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.NotEligible);
        }

        var definition = await definitionRepository.GetActiveLevelUpAsync(
            targetLanguage.TargetLanguageCode,
            targetLanguage.CurrentLevel,
            cancellationToken);
        if (definition is null)
        {
            return Result.Failure<AttemptDetailResponse>(AssessmentErrors.DefinitionNotFound);
        }

        var attempt = UserAssessmentAttempt.Start(
            request.UserId,
            definition.Id,
            definition.TargetLanguageCode,
            targetLanguage.CurrentLevel,
            utcNow);
        await attemptRepository.AddAsync(attempt, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return attempt.ToDetailResponse(definition);
    }
}
