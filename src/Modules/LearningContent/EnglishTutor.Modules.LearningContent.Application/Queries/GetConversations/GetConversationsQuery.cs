using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetConversations;

public sealed record GetConversationsQuery(
    Guid UserId,
    int Page,
    int PageSize,
    string? Level,
    int? Difficulty,
    string? TargetLanguageCode) : IQuery<IReadOnlyList<ConversationListResponse>>;
