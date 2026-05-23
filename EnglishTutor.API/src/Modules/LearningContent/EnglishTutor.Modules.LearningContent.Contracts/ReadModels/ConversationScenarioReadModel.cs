namespace EnglishTutor.Modules.LearningContent.Contracts.ReadModels;

public sealed record ConversationScenarioReadModel(
    Guid Id,
    string Title,
    string Setting,
    string Level,
    int Difficulty,
    IReadOnlyList<ConversationLineReadModel> Lines);

public sealed record ConversationLineReadModel(
    int Order,
    string Speaker,
    string Text,
    string? ExpectedResponseHint,
    string? AudioUrl);
