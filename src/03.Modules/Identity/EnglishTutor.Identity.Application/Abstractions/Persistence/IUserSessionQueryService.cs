using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Identity.Application.Queries.GetUserSessions;

namespace EnglishTutor.Identity.Application.Abstractions.Persistence;

public interface IUserSessionQueryService
{
    Task<PagedResult<UserSessionResult>> GetActiveSessionsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
