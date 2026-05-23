using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Contracts.Permissions;

namespace EnglishTutor.Modules.Auth.Application.Commands.DeletePermission;

public sealed class DeleteAuthPermissionCommandHandler(
    IAuthRolePermissionRepository repository,
    IAuthUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAuthPermissionCommand>
{
    public async Task<Result> Handle(DeleteAuthPermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await repository.GetPermissionByIdAsync(request.PermissionId, cancellationToken);
        if (permission is null)
        {
            return Result.Failure(AuthErrors.PermissionNotFound(request.PermissionId));
        }

        if (PermissionCodes.All.Contains(permission.Code, StringComparer.OrdinalIgnoreCase))
        {
            return Result.Failure(AuthErrors.SystemPermissionCannotBeDeleted);
        }

        if (await repository.IsPermissionAssignedAsync(permission.Id, cancellationToken))
        {
            return Result.Failure(AuthErrors.PermissionAssignedToRoles);
        }

        repository.RemovePermission(permission);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
