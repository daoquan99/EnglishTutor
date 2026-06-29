using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.Practice.Contracts.Dtos;

namespace EnglishTutor.Practice.Application.Sessions.Commands.CreatePracticeLiveAccess;

public sealed record CreatePracticeLiveAccessCommand(
    Guid UserId,
    Guid SessionId,
    string? VoiceId,
    string NativeLanguageCode,
    string TargetLanguageCode) : ICommand<PracticeLiveAccessResult>;
