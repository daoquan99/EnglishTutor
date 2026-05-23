using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Assessments.Application.Abstractions;
using EnglishTutor.Modules.Assessments.Application.Shared.DTOs;
using EnglishTutor.Modules.Assessments.Application.Shared.Errors;
using EnglishTutor.Modules.Assessments.Application.Shared.Mappers;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.Assessments.Application.Queries.GetAvailableAssessments;

public sealed class GetAvailableAssessmentsQueryHandler(
    IAssessmentDefinitionRepository definitionRepository,
    IUserTargetLanguageReader targetLanguageReader)
    : IQueryHandler<GetAvailableAssessmentsQuery, IReadOnlyList<AvailableAssessmentResponse>>
{
    public async Task<Result<IReadOnlyList<AvailableAssessmentResponse>>> Handle(GetAvailableAssessmentsQuery request, CancellationToken cancellationToken)
    {
        var targetLanguages = await targetLanguageReader.GetByUserIdAsync(request.UserId, cancellationToken);
        var targetLanguage = targetLanguages.FirstOrDefault(language =>
                language.IsActive &&
                (string.IsNullOrWhiteSpace(request.TargetLanguageCode) ||
                    language.TargetLanguageCode.Equals(request.TargetLanguageCode.Trim(), StringComparison.OrdinalIgnoreCase)))
            ?? targetLanguages.FirstOrDefault();
        if (targetLanguage is null)
        {
            return Result.Failure<IReadOnlyList<AvailableAssessmentResponse>>(AssessmentErrors.TargetLanguageNotFound);
        }

        var assessments = await definitionRepository.ListAvailableAsync(
            request.UserId,
            targetLanguage.TargetLanguageCode,
            targetLanguage.CurrentLevel,
            cancellationToken);

        return assessments.Select(assessment => assessment.ToAvailableResponse()).ToArray();
    }
}
