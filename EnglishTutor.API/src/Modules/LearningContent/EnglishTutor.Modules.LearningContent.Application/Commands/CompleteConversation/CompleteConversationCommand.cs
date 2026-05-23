using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.LearningContent.Application.Shared.DTOs;

namespace EnglishTutor.Modules.LearningContent.Application.Commands.CompleteConversation;

public sealed record CompleteConversationCommand(Guid UserId, Guid ScenarioId, int DurationSeconds) : ICommand<LearningPathCardResponse>;
