using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Commands.StartSession;

public sealed record StartSpeakingSessionCommand(
    Guid UserId,
    string SessionType,
    string? Topic,
    Guid? ConversationScenarioId) : ICommand<SpeakingSessionResponse>;
