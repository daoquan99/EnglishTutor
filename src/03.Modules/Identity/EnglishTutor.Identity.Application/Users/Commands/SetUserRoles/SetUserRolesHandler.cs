using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;

namespace EnglishTutor.Identity.Application.Users.Commands.SetUserRoles;

public sealed class SetUserRolesHandler : ICommandHandler<SetUserRolesCommand, UserDetailResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IUserManagementQueryService _queryService;

    public SetUserRolesHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IIdentityUnitOfWork unitOfWork,
        IUserManagementQueryService queryService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _queryService = queryService;
    }

    public async Task<Result<UserDetailResult>> Handle(
        SetUserRolesCommand request,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserDetailResult>(UserErrors.NotFound(request.UserId));
        }

        var requestedRoles = request.Roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (UserManagementRolePolicy.ContainsPrivilegedRole(requestedRoles) && !request.ActorIsOwner)
        {
            return Result.Failure<UserDetailResult>(UserErrors.PrivilegedRoleRequiresOwner());
        }

        var currentRoles = await _roleRepository.GetNamesByIdsAsync(user.RoleIds, cancellationToken);
        if (UserManagementRolePolicy.ContainsPrivilegedRole(currentRoles) && !request.ActorIsOwner)
        {
            return Result.Failure<UserDetailResult>(UserErrors.PrivilegedUserRequiresOwner());
        }

        var roles = await _roleRepository.GetByNamesAsync(requestedRoles, cancellationToken);
        var roleByName = roles.ToDictionary(role => role.Name, StringComparer.Ordinal);
        foreach (var roleName in requestedRoles)
        {
            if (!roleByName.ContainsKey(roleName))
            {
                return Result.Failure<UserDetailResult>(RoleErrors.NotFoundByName(roleName));
            }
        }

        await _userRepository.ReplaceRolesAsync(
            user.Id,
            roles.Select(role => role.Id).ToArray(),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _queryService.GetUserAsync(user.Id, cancellationToken);
        return Result.Success(updated!);
    }
}
