using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Domain.Aggregates.Users.Errors;

namespace EnglishTutor.Identity.Application.Users.Queries.GetUser;

public sealed class GetUserHandler : IQueryHandler<GetUserQuery, UserDetailResult>
{
    private readonly IUserManagementQueryService _queryService;

    public GetUserHandler(IUserManagementQueryService queryService)
    {
        _queryService = queryService;
    }

    public async Task<Result<UserDetailResult>> Handle(
        GetUserQuery request,
        CancellationToken cancellationToken)
    {
        var user = await _queryService.GetUserAsync(request.UserId, cancellationToken);
        return user is null
            ? Result.Failure<UserDetailResult>(UserErrors.NotFound(request.UserId))
            : Result.Success(user);
    }
}
