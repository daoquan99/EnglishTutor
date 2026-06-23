using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EnglishTutor.Identity.Application.Queries.GetUserSessions;

public sealed class GetUserSessionsQueryHandler
    : IQueryHandler<GetUserSessionsQuery, IReadOnlyList<UserSessionResult>>
{
    private readonly IUserSessionRepository _userSessionRepository;

    public GetUserSessionsQueryHandler(IUserSessionRepository userSessionRepository)
    {
        _userSessionRepository = userSessionRepository;
    }

    public async Task<Result<IReadOnlyList<UserSessionResult>>> Handle(
        GetUserSessionsQuery request,
        CancellationToken cancellationToken)
    {
        var sessions = await _userSessionRepository.GetActiveSessionsByUserIdAsync(request.UserId, cancellationToken);

        var results = sessions
            .Select(s => new UserSessionResult(
                Id: s.Id,
                DeviceId: s.Device.DeviceId,
                DeviceName: s.Device.DeviceName,
                UserAgentHash: s.Device.UserAgentHash,
                IpAddressHash: s.Device.IpAddressHash,
                CreatedAtUtc: s.CreatedAtUtc,
                LastSeenAtUtc: s.LastSeenAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<UserSessionResult>>(results);
    }
}
