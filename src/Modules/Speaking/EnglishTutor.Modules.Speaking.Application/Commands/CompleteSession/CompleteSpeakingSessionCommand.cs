using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Speaking.Application.DTOs;

namespace EnglishTutor.Modules.Speaking.Application.Commands.CompleteSession;

public sealed record CompleteSpeakingSessionCommand(
    Guid UserId,
    Guid SessionId) : ICommand<SpeakingSessionSummaryResponse>;
