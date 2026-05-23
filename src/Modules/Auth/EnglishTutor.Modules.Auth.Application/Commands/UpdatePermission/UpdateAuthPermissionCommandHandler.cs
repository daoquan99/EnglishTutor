using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Application.Shared.Mappers;
using EnglishTutor.Modules.Auth.Contracts.Permissions;

namespace EnglishTutor.Modules.Auth.Application.Commands.UpdatePermission;

public sealed class UpdateAuthPermissionCommandHandler(
    IAuthRolePermissionRepository repository,
    IAuthUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateAuthPermissionCommand, AuthPermissionResponse>
{
    public async Task<Result<AuthPermissionResponse>> Handle(
        UpdateAuthPermissionCommand request,
        CancellationToken cancellationToken)
    {
        var permission = await repository.GetPermissionByIdAsync(request.PermissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Failure<AuthPermissionResponse>(AuthErrors.PermissionNotFound(request.PermissionId));
        }

        if (!request.IsEnabled && PermissionCodes.All.Contains(permission.Code, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure<AuthPermissionResponse>(AuthErrors.SystemPermissionCannotBeDisabled);
        }

        var utcNow = dateTimeProvider.UtcNow;
        permission.UpdateDescription(request.Description, utcNow);

        if (request.IsEnabled)
        {
            permission.Enable(utcNow);
        }
        else
        {
            permission.Disable(utcNow);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return permission.ToResponse();
    }
}
