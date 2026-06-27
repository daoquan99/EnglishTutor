using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Queries.GetUserSessions;
using FluentAssertions;

namespace Identity.UnitTests;

public sealed class GetUserSessionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ValidPage_ReturnsPagedProjection()
    {
        var expected = PagedResult<UserSessionResult>.Create(
            items:
            [
                new UserSessionResult(
                    Id: Guid.NewGuid(),
                    DeviceId: "device",
                    DeviceName: "Browser",
                    UserAgentHash: "ua-hash",
                    IpAddressHash: "ip-hash",
                    CreatedAtUtc: DateTime.UtcNow,
                    LastSeenAtUtc: DateTime.UtcNow)
            ],
            totalCount: 1,
            page: 1,
            pageSize: 20);
        var queryService = new FakeUserSessionQueryService(expected);
        var handler = new GetUserSessionsQueryHandler(queryService);

        var result = await handler.Handle(
            new GetUserSessionsQuery(
                UserId: Guid.NewGuid(),
                Page: 1,
                PageSize: 20),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(expected);
    }

    private sealed class FakeUserSessionQueryService : IUserSessionQueryService
    {
        private readonly PagedResult<UserSessionResult> _result;

        public FakeUserSessionQueryService(PagedResult<UserSessionResult> result)
        {
            _result = result;
        }

        public Task<PagedResult<UserSessionResult>> GetActiveSessionsAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
            Task.FromResult(_result);
    }
}
