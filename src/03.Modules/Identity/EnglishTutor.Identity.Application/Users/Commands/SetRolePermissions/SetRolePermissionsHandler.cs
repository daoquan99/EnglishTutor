using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;

namespace EnglishTutor.Identity.Application.Users.Commands.SetRolePermissions;

public sealed class SetRolePermissionsHandler : ICommandHandler<SetRolePermissionsCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public SetRolePermissionsHandler(
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleResult>> Handle(
        SetRolePermissionsCommand request,
        CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role is null)
        {
            return Result.Failure<RoleResult>(RoleErrors.NotFound(request.RoleId));
        }

        var permissionCodes = request.PermissionCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var permissions = await _roleRepository.GetPermissionsByCodesAsync(permissionCodes, cancellationToken);
        var permissionByCode = permissions.ToDictionary(permission => permission.Code, StringComparer.Ordinal);
        foreach (var code in permissionCodes)
        {
            if (!permissionByCode.ContainsKey(code))
            {
                return Result.Failure<RoleResult>(RoleErrors.PermissionNotFound(code));
            }
        }

        await _roleRepository.ReplacePermissionsAsync(
            role.Id,
            permissions.Select(permission => permission.Id).ToArray(),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RoleResult(
            Id: role.Id,
            Name: role.Name,
            DisplayName: role.DisplayName,
            Priority: role.Priority,
            PermissionCodes: permissionCodes));
    }
}
