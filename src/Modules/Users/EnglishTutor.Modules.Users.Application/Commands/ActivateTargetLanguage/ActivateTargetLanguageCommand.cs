using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.DTOs;

namespace EnglishTutor.Modules.Users.Application.Commands.ActivateTargetLanguage;

public sealed record ActivateTargetLanguageCommand(Guid UserId, Guid TargetLanguageId) : ICommand<TargetLanguageResponse>;
