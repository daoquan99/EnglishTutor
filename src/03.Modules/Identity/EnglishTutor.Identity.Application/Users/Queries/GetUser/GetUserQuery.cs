using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Identity.Application.Users.Queries.GetUser;

public sealed record GetUserQuery(Guid UserId) : IQuery<UserDetailResult>;
