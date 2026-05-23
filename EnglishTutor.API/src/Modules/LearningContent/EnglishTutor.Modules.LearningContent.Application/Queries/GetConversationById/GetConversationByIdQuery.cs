using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.LearningContent.Application.Queries.GetConversationById;

public sealed record GetConversationByIdQuery(Guid UserId, Guid ScenarioId) : IQuery<ConversationDetailResponse>;
