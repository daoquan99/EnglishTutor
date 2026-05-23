namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetConversationById;

public sealed record ConversationDetailResponse(
    Guid Id,
    string TargetLanguageCode,
    string Level,
    string Title,
    string Description,
    string Setting,
    int Difficulty,
    int EstimatedMinutes,
    IReadOnlyList<ConversationLineResponse> Lines);

public sealed record ConversationLineResponse(
    Guid Id,
    int Order,
    string Speaker,
    string Text,
    string? ExpectedResponseHint,
    string? AudioUrl,
    string? Notes);
