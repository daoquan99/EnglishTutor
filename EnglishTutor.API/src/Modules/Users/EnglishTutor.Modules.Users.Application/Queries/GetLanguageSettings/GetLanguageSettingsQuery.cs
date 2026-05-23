using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.Shared.DTOs;

namespace EnglishTutor.Modules.Users.Application.Queries.GetLanguageSettings;

public sealed record GetLanguageSettingsQuery(Guid UserId) : IQuery<LanguageSettingsResponse>;
