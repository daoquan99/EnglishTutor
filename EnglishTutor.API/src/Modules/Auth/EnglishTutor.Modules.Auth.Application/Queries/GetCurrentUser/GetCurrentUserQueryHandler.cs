using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;

namespace EnglishTutor.Modules.Auth.Application.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(
    ICurrentUser currentUser,
    IAuthRepository authRepository,
    IAuthPermissionRepository authPermissionRepository)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await authRepository.GetByIdAsync(currentUser.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<CurrentUserResponse>(AuthErrors.UserNotFound(currentUser.UserId));
        }

        var permissions = await authPermissionRepository.GetPermissionCodesByUserIdAsync(user.Id, cancellationToken);
        var hasPermissions = permissions.Count > 0;

        IReadOnlyList<string>? roles = null;
        if (hasPermissions)
        {
            roles = await authPermissionRepository.GetRoleNamesByUserIdAsync(user.Id, cancellationToken);
        }

        return new CurrentUserResponse
        {
            UserId = user.Id,
            Email = user.Email.Value,
            DisplayName = user.DisplayName,
            Roles = hasPermissions ? roles : null,
            Permissions = hasPermissions ? permissions : null
        };
    }
}
