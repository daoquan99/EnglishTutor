using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.LearningContent.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.Mappers;
using EnglishTutor.Modules.Users.Contracts.Readers;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetConversations;

public sealed class GetConversationsQueryHandler(
    IConversationScenarioRepository scenarioRepository,
    IUserLanguageSettingsReader languageSettingsReader)
    : IQueryHandler<GetConversationsQuery, IReadOnlyList<ConversationListResponse>>
{
    public async Task<Result<IReadOnlyList<ConversationListResponse>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var settings = await languageSettingsReader.GetByUserIdAsync(request.UserId, cancellationToken);
        var uiLanguageCode = settings?.UiLanguageCode ?? "en";
        var targetLanguageCode = request.TargetLanguageCode ?? settings?.TargetLanguageCode;
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : Math.Min(request.PageSize, 100);

        var scenarios = await scenarioRepository.ListPublishedAsync(
            page,
            pageSize,
            request.Level,
            request.Difficulty,
            targetLanguageCode,
            cancellationToken);

        return Result.Success<IReadOnlyList<ConversationListResponse>>(scenarios.Select(scenario => scenario.ToListResponse(uiLanguageCode)).ToArray());
    }
}
