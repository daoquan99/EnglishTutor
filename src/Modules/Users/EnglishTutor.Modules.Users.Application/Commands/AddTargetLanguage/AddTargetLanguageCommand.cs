using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.DTOs;

namespace EnglishTutor.Modules.Users.Application.Commands.AddTargetLanguage;

public sealed record AddTargetLanguageCommand(
    Guid UserId,
    string TargetLanguageCode,
    string CurrentLevel,
    string TargetLevel) : ICommand<TargetLanguageResponse>;
