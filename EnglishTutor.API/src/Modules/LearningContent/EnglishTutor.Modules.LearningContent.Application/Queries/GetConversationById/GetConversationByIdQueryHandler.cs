using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.Errors;
using EnglishTutor.Modules.LearningContent.Application.Shared.Mappers;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetConversationById;

public sealed class GetConversationByIdQueryHandler(
    IConversationScenarioRepository scenarioRepository,
    IUserLanguageSettingsReader languageSettingsReader)
    : IQueryHandler<GetConversationByIdQuery, ConversationDetailResponse>
{
    public async Task<Result<ConversationDetailResponse>> Handle(GetConversationByIdQuery request, CancellationToken cancellationToken)
    {
        var scenario = await scenarioRepository.GetByIdWithDetailsAsync(request.ScenarioId, cancellationToken);
        if (scenario is null)
        {
            return Result.Failure<ConversationDetailResponse>(LearningContentErrors.ConversationScenarioNotFound(request.ScenarioId));
        }

        if (!scenario.IsPublished)
        {
            return Result.Failure<ConversationDetailResponse>(LearningContentErrors.ContentNotPublished);
        }

        var settings = await languageSettingsReader.GetByUserIdAsync(request.UserId, cancellationToken);
        return scenario.ToDetailResponse(settings?.UiLanguageCode ?? "en");
    }
}
