using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;
using EnglishTutor.Modules.Auth.Application.Shared.Mappers;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;

namespace EnglishTutor.Modules.Auth.Application.Commands.UpdateRole;

public sealed class UpdateAuthRoleCommandHandler(
    IAuthRolePermissionRepository repository,
    IAuthUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<UpdateAuthRoleCommand, AuthRoleResponse>
{
    public async Task<Result<AuthRoleResponse>> Handle(
        UpdateAuthRoleCommand request,
        CancellationToken cancellationToken)
    {
        var role = await repository.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<AuthRoleResponse>(AuthErrors.RoleNotFound(request.RoleId));
        }

        if (role.IsSystem)
        {
            return Result.Failure<AuthRoleResponse>(AuthErrors.SystemRoleCannotBeModified);
        }

        var existingRole = await repository.GetRoleByNameAsync(request.Name, cancellationToken);
        if (existingRole is not null && existingRole.Id != role.Id)
        {
            return Result.Failure<AuthRoleResponse>(AuthErrors.RoleNameAlreadyExists);
        }

        var distinctPermissionIds = (request.PermissionIds ?? Array.Empty<Guid>()).Distinct().ToArray();
        if (!await repository.ArePermissionIdsValidAsync(distinctPermissionIds, cancellationToken))
        {
            return Result.Failure<AuthRoleResponse>(AuthErrors.InvalidPermissionSelection);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var previousIsEnabled = role.IsEnabled;
        role.Update(request.Name, request.Description, request.IsEnabled, utcNow);

        var currentAssignments = await repository.GetRolePermissionAssignmentsAsync(role.Id, cancellationToken);
        var currentPermissionIds = currentAssignments.Select(assignment => assignment.PermissionId).ToHashSet();
        var requestedPermissionIds = distinctPermissionIds.ToHashSet();

        foreach (var assignment in currentAssignments.Where(assignment => !requestedPermissionIds.Contains(assignment.PermissionId)))
        {
            repository.RemoveRolePermission(assignment);
        }

        foreach (var permissionId in requestedPermissionIds.Where(permissionId => !currentPermissionIds.Contains(permissionId)))
        {
            await repository.AddRolePermissionAsync(
                AuthRolePermission.Create(role.Id, permissionId, utcNow),
                cancellationToken);
        }

        var effectiveSetChanged =
            !currentPermissionIds.SetEquals(requestedPermissionIds) ||
            previousIsEnabled != role.IsEnabled;

        if (effectiveSetChanged)
        {
            var affectedUserIds = await repository.GetUserIdsAssignedToRoleAsync(role.Id, cancellationToken);
            role.RaisePermissionsChanged(affectedUserIds);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var permissions = await repository.GetPermissionsByRoleIdAsync(role.Id, cancellationToken);
        return role.ToResponse(permissions);
    }
}
