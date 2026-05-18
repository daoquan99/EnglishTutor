using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Commands.StartSession;

public sealed record StartSpeakingSessionCommand(
    Guid UserId,
    string SessionType,
    string? Topic) : ICommand<SpeakingSessionResponse>;
