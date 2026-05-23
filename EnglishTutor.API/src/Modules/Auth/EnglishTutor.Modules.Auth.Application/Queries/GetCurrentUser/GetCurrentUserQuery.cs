using EnglishTutor.BuildingBlocks.Application.Abstractions;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<CurrentUserResponse>;
