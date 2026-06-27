using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Queries.GetUserSessions;
using EnglishTutor.Identity.Domain.Aggregates.Sessions;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Queries;

internal sealed class UserSessionQueryService : IUserSessionQueryService
{
    private const int MaxPageSize = 100;
    private const int MinPageSize = 1;
    private const int MinPage = 1;

    private readonly IdentityDbContext _db;

    public UserSessionQueryService(IdentityDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<UserSessionResult>> GetActiveSessionsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePage = Math.Max(MinPage, page);
        var safePageSize = Math.Clamp(pageSize, MinPageSize, MaxPageSize);
        var query = _db.Set<UserSession>()
            .AsNoTracking()
            .Where(session => session.UserId == userId && session.RevokedAtUtc == null);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(session => session.LastSeenAtUtc)
            .ThenByDescending(session => session.Id)
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .Select(session => new UserSessionResult(
                Id: session.Id,
                DeviceId: session.Device.DeviceId,
                DeviceName: session.Device.DeviceName,
                UserAgentHash: session.Device.UserAgentHash,
                IpAddressHash: session.Device.IpAddressHash,
                CreatedAtUtc: session.CreatedAtUtc,
                LastSeenAtUtc: session.LastSeenAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<UserSessionResult>.Create(
            items: items,
            totalCount: totalCount,
            page: safePage,
            pageSize: safePageSize);
    }
}
