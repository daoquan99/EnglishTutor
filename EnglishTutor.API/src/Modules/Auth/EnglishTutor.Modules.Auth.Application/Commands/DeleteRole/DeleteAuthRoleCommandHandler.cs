using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.BuildingBlocks.Application.Results;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.Errors;

namespace EnglishTutor.Modules.Auth.Application.Commands.DeleteRole;

public sealed class DeleteAuthRoleCommandHandler(
    IAuthRolePermissionRepository repository,
    IAuthUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAuthRoleCommand>
{
    public async Task<Result> Handle(DeleteAuthRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await repository.GetRoleByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure(AuthErrors.RoleNotFound(request.RoleId));
        }

        if (role.IsSystem)
        {
            return Result.Failure(AuthErrors.SystemRoleCannotBeDeleted);
        }

        if (await repository.IsRoleAssignedToAnyUserAsync(role.Id, cancellationToken))
        {
            return Result.Failure(AuthErrors.RoleAssignedToUsers);
        }

        var assignments = await repository.GetRolePermissionAssignmentsAsync(role.Id, cancellationToken);
        foreach (var assignment in assignments)
        {
            repository.RemoveRolePermission(assignment);
        }

        repository.RemoveRole(role);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
