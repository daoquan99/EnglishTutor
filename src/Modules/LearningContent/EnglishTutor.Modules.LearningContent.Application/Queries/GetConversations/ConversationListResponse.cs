namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetConversations;

public sealed record ConversationListResponse(
    Guid Id,
    string TargetLanguageCode,
    string Level,
    string Title,
    string Description,
    string Setting,
    int Difficulty,
    int EstimatedMinutes);
