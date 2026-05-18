using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Users.Application.DTOs;

namespace EnglishTutor.Modules.Users.Application.Queries.GetTargetLanguages;

public sealed record GetTargetLanguagesQuery(Guid UserId) : IQuery<IReadOnlyList<TargetLanguageResponse>>;
