using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.BuildingBlocks.Application.Queries;

namespace EnglishTutor.Identity.Application.Queries.GetUserSessions;

public sealed record GetUserSessionsQuery(
    Guid UserId,
    int Page,
    int PageSize) : IQuery<PagedResult<UserSessionResult>>;
