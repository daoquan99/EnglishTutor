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

        var roles = await authPermissionRepository.GetRoleNamesByUserIdAsync(user.Id, cancellationToken);
        var permissions = await authPermissionRepository.GetPermissionCodesByUserIdAsync(user.Id, cancellationToken);

        return new CurrentUserResponse(user.Id, user.Email.Value, user.DisplayName, roles, permissions);
    }
}
