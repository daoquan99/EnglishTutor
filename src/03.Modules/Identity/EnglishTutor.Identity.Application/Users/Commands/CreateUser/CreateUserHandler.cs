using EnglishTutor.BuildingBlocks.Application.Commands;
using EnglishTutor.BuildingBlocks.Domain.Results;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;
using EnglishTutor.Identity.Domain.Aggregates.Roles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Errors;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;

namespace EnglishTutor.Identity.Application.Users.Commands.CreateUser;

public sealed class CreateUserHandler : ICommandHandler<CreateUserCommand, UserDetailResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IIdentityUnitOfWork _unitOfWork;
    private readonly IUserManagementQueryService _queryService;

    public CreateUserHandler(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPasswordHasher passwordHasher,
        IIdentityUnitOfWork unitOfWork,
        IUserManagementQueryService queryService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _queryService = queryService;
    }

    public async Task<Result<UserDetailResult>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        var email = Email.Create(request.Email);
        var existing = await _userRepository.FindByEmailAsync(
            email,
            includeDeleted: true,
            cancellationToken);
        if (existing is not null)
        {
            return Result.Failure<UserDetailResult>(UserErrors.AlreadyExists(email.Value));
        }

        var requestedRoles = NormalizeRoles(request.Roles);
        if (UserManagementRolePolicy.ContainsPrivilegedRole(requestedRoles) && !request.ActorIsOwner)
        {
            return Result.Failure<UserDetailResult>(UserErrors.PrivilegedRoleRequiresOwner());
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

        var user = User.Create(
            email: email,
            passwordHash: HashedPassword.FromNewHash(_passwordHasher.HashPassword(request.Password)),
            displayName: request.DisplayName ?? email.Value,
            roleIds: roles.Select(role => role.Id),
            createdByUserId: request.CreatedByUserId);

        if (!request.IsActive)
        {
            user.Deactivate();
        }

        _userRepository.Add(user);
        await _userRepository.ReplaceRolesAsync(
            user.Id,
            roles.Select(role => role.Id).ToArray(),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _queryService.GetUserAsync(user.Id, cancellationToken);
        return Result.Success(created!);
    }

    private static IReadOnlyList<string> NormalizeRoles(IReadOnlyList<string> roles)
    {
        var normalized = roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => role.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return normalized.Length == 0 ? [Role.WellKnownNames.User] : normalized;
    }
}
