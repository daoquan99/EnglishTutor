using EnglishTutor.BuildingBlocks.Application.CurrentUser;
using EnglishTutor.BuildingBlocks.Application.Queries;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;

namespace EnglishTutor.Identity.Application.Queries.GetCurrentUser;

/// <summary>
/// Handles <see cref="GetCurrentUserQuery"/>: returns the
/// <see cref="CurrentUserResult"/> snapshot for the JWT-authenticated
/// caller. Used by the <c>GET /api/me</c> endpoint.
/// <para>
/// Persistence-agnostic: depends only on Application abstractions
/// (<c>IUserRepository</c>, <c>IRoleRepository</c>). It does NOT
/// reference <c>IIdentityModuleDbContext</c>,
/// <c>the DbContext abstraction</c>, or any other Infrastructure type.
/// </para>
/// <para>
/// Soft-deleted and inactive users return
/// <c>Identity.UserNotFound</c> (matches the pre-Slice 2.6 behavior).
/// </para>
/// </summary>
public sealed class GetCurrentUserQueryHandler : IQueryHandler<GetCurrentUserQuery, CurrentUserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICurrentUser _currentUser;

    public GetCurrentUserQueryHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICurrentUser currentUser)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
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
                new Error(
                    "Identity.NotAuthenticated",
                    "Current user is not authenticated."));
        }

        // Exclude soft-deleted users from the /me response.
        var user = await _userRepository.GetByIdAsync(userId.Value, includeDeleted: false, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return Result.Failure<CurrentUserResult>(
                new Error(
                    "Identity.UserNotFound",
                    "Current user was not found or is inactive."));
        }

        var roleNames = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);

        return Result.Success(new CurrentUserResult(
            user.Id,
            user.Email.Value,
            user.DisplayName,
            roleNames));
    }
}
