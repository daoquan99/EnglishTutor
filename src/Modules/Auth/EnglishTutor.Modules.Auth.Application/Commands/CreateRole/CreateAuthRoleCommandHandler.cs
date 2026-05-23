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

namespace EnglishTutor.Modules.Auth.Application.Commands.CreateRole;

public sealed class CreateAuthRoleCommandHandler(
    IAuthRolePermissionRepository repository,
    IAuthUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateAuthRoleCommand, AuthRoleResponse>
{
    public async Task<Result<AuthRoleResponse>> Handle(
        CreateAuthRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (await repository.GetRoleByNameAsync(request.Name, cancellationToken) is not null)
        {
            return Result.Failure<AuthRoleResponse>(AuthErrors.RoleNameAlreadyExists);
        }

        var distinctPermissionIds = (request.PermissionIds ?? Array.Empty<Guid>()).Distinct().ToArray();
        if (!await repository.ArePermissionIdsValidAsync(distinctPermissionIds, cancellationToken))
        {
            return Result.Failure<AuthRoleResponse>(AuthErrors.InvalidPermissionSelection);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var role = AuthRole.Create(request.Name, request.Description, isSystem: false, utcNow);

        if (!request.IsEnabled)
        {
            role.Disable(utcNow);
        }

        await repository.AddRoleAsync(role, cancellationToken);

        foreach (var permissionId in distinctPermissionIds)
        {
            await repository.AddRolePermissionAsync(
                AuthRolePermission.Create(role.Id, permissionId, utcNow),
                cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var permissions = await repository.GetPermissionsByRoleIdAsync(role.Id, cancellationToken);
        return role.ToResponse(permissions);
    }
}
