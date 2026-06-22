using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Queries.GetCurrentUser;
using EnglishTutor.Identity.Application.Services;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using MediatR;

namespace EnglishTutor.Identity.Infrastructure.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResult>>
{
    private readonly IIdentityModuleDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserQueryHandler(IIdentityModuleDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Result<CurrentUserResult>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (!userId.HasValue)
        {
            return Result.Failure<CurrentUserResult>(
                new EnglishTutor.BuildingBlocks.Domain.Results.Error(
                    "Identity.NotAuthenticated",
                    "Current user is not authenticated."));
        }

        var user = await _db.FindUserWithRolesAsync(userId.Value, includeDeleted: false, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return Result.Failure<CurrentUserResult>(
                new EnglishTutor.BuildingBlocks.Domain.Results.Error(
                    "Identity.UserNotFound",
                    "Current user was not found or is inactive."));
        }

        var roleNames = await _db.GetRoleNamesAsync(user.RoleIds, cancellationToken);

        return Result.Success(new CurrentUserResult(
            user.Id,
            user.Email.Value,
            user.DisplayName,
            roleNames));
    }
}
