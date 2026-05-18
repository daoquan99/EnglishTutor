using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.DTOs;

namespace EnglishTutor.Modules.Users.Application.Commands.UpdateLanguageSettings;

public sealed record UpdateLanguageSettingsCommand(
    Guid UserId,
    string NativeLanguageCode,
    string UiLanguageCode,
    string ExplanationLanguageCode,
    string ActiveTargetLanguageCode) : ICommand<LanguageSettingsResponse>;
