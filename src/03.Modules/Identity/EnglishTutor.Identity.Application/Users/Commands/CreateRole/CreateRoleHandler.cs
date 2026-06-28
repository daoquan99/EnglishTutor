using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;
using EnglishTutor.Identity.Domain.Aggregates.Roles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;

namespace EnglishTutor.Identity.Application.Users.Commands.CreateRole;

public sealed class CreateRoleHandler : ICommandHandler<CreateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;

    public CreateRoleHandler(IRoleRepository roleRepository, IIdentityUnitOfWork unitOfWork)
    {
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RoleResult>> Handle(
        CreateRoleCommand request,
        CancellationToken cancellationToken)
    {
        var roleName = request.Name.Trim();
        var existing = await _roleRepository.GetByNameAsync(roleName, cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<RoleResult>(RoleErrors.AlreadyExists(roleName));
        }

        var permissionCodes = NormalizePermissionCodes(request.PermissionCodes);
        var permissions = await _roleRepository.GetPermissionsByCodesAsync(permissionCodes, cancellationToken);
        var permissionByCode = permissions.ToDictionary(permission => permission.Code, StringComparer.Ordinal);
        foreach (var code in permissionCodes)
        {
            if (!permissionByCode.ContainsKey(code))
            {
                return Result.Failure<RoleResult>(RoleErrors.PermissionNotFound(code));
            }
        }

        var role = new Role(
            name: roleName,
            displayName: request.DisplayName,
            priority: request.Priority);

        _roleRepository.Add(role);
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

    private static IReadOnlyList<string> NormalizePermissionCodes(IReadOnlyList<string> permissionCodes) =>
        permissionCodes
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
}
