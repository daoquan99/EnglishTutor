using EnglishTutor.BuildingBlocks.Application.Pagination;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Application.Abstractions.Persistence;
using EnglishTutor.Identity.Application.Queries.GetCurrentUser;
using EnglishTutor.Identity.Application.Users.Commands.ChangeMyPassword;
using EnglishTutor.Identity.Application.Users.Commands.CreateRole;
using EnglishTutor.Identity.Application.Users.Commands.SetRolePermissions;
using EnglishTutor.Identity.Application.Users.Commands.CreateUser;
using EnglishTutor.Identity.Application.Users.Commands.SetUserRoles;
using EnglishTutor.Identity.Application.Users.Queries.ListPermissions;
using EnglishTutor.Identity.Application.Users.Queries.GetUser;
using EnglishTutor.Identity.Application.Users.Queries.ListRoles;
using EnglishTutor.Identity.Application.Users.Queries.ListUsers;
using EnglishTutor.Identity.Domain.Aggregates.Roles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using FluentAssertions;

namespace EnglishTutor.Identity.UnitTests;

public sealed class UserManagementHandlersTests
{
    [Fact]
    public async Task CreateUser_Should_Block_NonOwner_From_Assigning_Admin()
    {
        var users = new FakeUserManagementUserRepository();
        var roles = new FakeUserManagementRoleRepository();
        var handler = new CreateUserHandler(
            users,
            roles,
            new FakeUserManagementPasswordHasher(),
            new FakeIdentityUnitOfWork(),
            new FakeUserManagementQueryService());

        var result = await handler.Handle(new CreateUserCommand(
            Email: "admin@example.com",
            Password: "password123",
            DisplayName: "Admin",
            Roles: [Role.WellKnownNames.Admin],
            IsActive: true,
            CreatedByUserId: Guid.NewGuid(),
            ActorIsOwner: false), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Identity.PrivilegedRoleRequiresOwner");
        users.Users.Should().BeEmpty();
    }

    [Fact]
    public async Task SetUserRoles_Should_Block_NonOwner_From_Modifying_Owner()
    {
        var ownerRole = new Role(Role.WellKnownNames.Owner, "Owner", priority: 100);
        var userRole = new Role(Role.WellKnownNames.User, "User", priority: 10);
        var user = User.Create(
            email: Email.Create("owner@example.com"),
            passwordHash: HashedPassword.FromNewHash("hash"),
            displayName: "Owner",
            roleIds: [ownerRole.Id],
            createdByUserId: null);

        var users = new FakeUserManagementUserRepository();
        users.Users.Add(user);
        var roles = new FakeUserManagementRoleRepository(ownerRole, userRole);

        var handler = new SetUserRolesHandler(
            users,
            roles,
            new FakeIdentityUnitOfWork(),
            new FakeUserManagementQueryService());

        var result = await handler.Handle(new SetUserRolesCommand(
            UserId: user.Id,
            Roles: [Role.WellKnownNames.User],
            ActorIsOwner: false), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Identity.PrivilegedUserRequiresOwner");
    }

    [Fact]
    public async Task ChangeMyPassword_Should_Return_Forbidden_For_Invalid_Current_Password()
    {
        var user = User.Create(
            email: Email.Create("user@example.com"),
            passwordHash: HashedPassword.FromNewHash("valid-hash"),
            displayName: "User",
            roleIds: [],
            createdByUserId: null);
        var users = new FakeUserManagementUserRepository();
        users.Users.Add(user);
        var unitOfWork = new FakeIdentityUnitOfWork();

        var handler = new ChangeMyPasswordHandler(
            users,
            new FakeUserManagementPasswordHasher(),
            unitOfWork);

        var result = await handler.Handle(new ChangeMyPasswordCommand(
            UserId: user.Id,
            CurrentPassword: "wrong",
            NewPassword: "new-password"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Identity.InvalidCurrentPassword");
        unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task CreateRole_Should_Return_NotFound_When_Permission_Code_Is_Unknown()
    {
        var roles = new FakeUserManagementRoleRepository();
        var unitOfWork = new FakeIdentityUnitOfWork();
        var handler = new CreateRoleHandler(roles, unitOfWork);

        var result = await handler.Handle(new CreateRoleCommand(
            Name: "Coach",
            DisplayName: "Coach",
            Priority: 20,
            PermissionCodes: ["identity.users.read"]), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Code.Should().Be("Identity.PermissionNotFound");
        unitOfWork.SavedChangesCount.Should().Be(0);
    }

    [Fact]
    public async Task SetRolePermissions_Should_Replace_Permissions_When_Codes_Exist()
    {
        var role = new Role("Coach", "Coach", priority: 20);
        var permission = new Permission("identity.users.read", "Identity", "Read users");
        var roles = new FakeUserManagementRoleRepository(role);
        roles.Permissions.Add(permission);
        var unitOfWork = new FakeIdentityUnitOfWork();
        var handler = new SetRolePermissionsHandler(
            roles,
            unitOfWork);

        var result = await handler.Handle(new SetRolePermissionsCommand(
            RoleId: role.Id,
            PermissionCodes: [permission.Code]), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        roles.ReplacedPermissions[role.Id].Should().ContainSingle().Which.Should().Be(permission.Id);
        unitOfWork.SavedChangesCount.Should().Be(1);
    }

    private sealed class FakeUserManagementUserRepository : IUserRepository
    {
        public List<User> Users { get; } = [];
        public Dictionary<Guid, IReadOnlyCollection<Guid>> ReplacedRoles { get; } = [];

        public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct) =>
            GetByIdAsync(userId, includeDeleted: false, ct);

        public Task<User?> GetByIdAsync(Guid userId, bool includeDeleted, CancellationToken ct) =>
            Task.FromResult(Users.FirstOrDefault(user => user.Id == userId));

        public Task<User?> FindByEmailAsync(Email email, bool includeDeleted, CancellationToken ct) =>
            Task.FromResult(Users.FirstOrDefault(user => user.Email.Value == email.Value));

        public void Add(User user) => Users.Add(user);

        public Task ReplaceRolesAsync(
            Guid userId,
            IReadOnlyCollection<Guid> roleIds,
            CancellationToken ct)
        {
            ReplacedRoles[userId] = roleIds;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserManagementRoleRepository : IRoleRepository
    {
        private readonly List<Role> _roles;

        public List<Permission> Permissions { get; } = [];
        public Dictionary<Guid, IReadOnlyCollection<Guid>> ReplacedPermissions { get; } = [];

        public FakeUserManagementRoleRepository(params Role[] roles)
        {
            _roles = roles.Length == 0
                ?
                [
                    new Role(Role.WellKnownNames.Owner, "Owner", priority: 100),
                    new Role(Role.WellKnownNames.Admin, "Admin", priority: 50),
                    new Role(Role.WellKnownNames.User, "User", priority: 10)
                ]
                : roles.ToList();
        }

        public Task<Role?> GetByIdAsync(Guid roleId, CancellationToken ct) =>
            Task.FromResult(_roles.FirstOrDefault(role => role.Id == roleId));

        public Task<Role?> GetByNameAsync(string name, CancellationToken ct) =>
            Task.FromResult(_roles.FirstOrDefault(role => role.Name == name));

        public Task<IReadOnlyList<Role>> ListAsync(CancellationToken ct) =>
            Task.FromResult((IReadOnlyList<Role>)_roles);

        public Task<IReadOnlyList<string>> GetPermissionCodesForRolesAsync(
            IReadOnlyCollection<Guid> roleIds,
            CancellationToken ct) =>
            Task.FromResult((IReadOnlyList<string>)[]);

        public Task<IReadOnlyList<string>> GetNamesByIdsAsync(
            IReadOnlyCollection<Guid> roleIds,
            CancellationToken ct)
        {
            var names = _roles
                .Where(role => roleIds.Contains(role.Id))
                .Select(role => role.Name)
                .ToArray();

            return Task.FromResult((IReadOnlyList<string>)names);
        }

        public Task<IReadOnlyList<Role>> GetByNamesAsync(
            IReadOnlyCollection<string> names,
            CancellationToken ct)
        {
            var roles = _roles
                .Where(role => names.Contains(role.Name))
                .ToArray();

            return Task.FromResult((IReadOnlyList<Role>)roles);
        }

        public Task<IReadOnlyList<Permission>> GetPermissionsByCodesAsync(
            IReadOnlyCollection<string> codes,
            CancellationToken ct)
        {
            var permissions = Permissions
                .Where(permission => codes.Contains(permission.Code))
                .ToArray();

            return Task.FromResult((IReadOnlyList<Permission>)permissions);
        }

        public Task ReplacePermissionsAsync(
            Guid roleId,
            IReadOnlyCollection<Guid> permissionIds,
            CancellationToken ct)
        {
            ReplacedPermissions[roleId] = permissionIds;
            return Task.CompletedTask;
        }

        public void Add(Role role) => _roles.Add(role);
    }

    private sealed class FakeUserManagementPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string plaintextPassword) => $"hash:{plaintextPassword}";

        public PasswordVerificationResult VerifyPassword(string plaintextPassword, string storedHash) =>
            plaintextPassword == "current"
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
    }

    private sealed class FakeUserManagementQueryService : IUserManagementQueryService
    {
        public Task<PagedResult<UserListItemResult>> ListUsersAsync(
            int page,
            int pageSize,
            string? search,
            string? status,
            string? role,
            CancellationToken cancellationToken) =>
            Task.FromResult(PagedResult<UserListItemResult>.Create(
                items: [],
                totalCount: 0,
                page: page,
                pageSize: pageSize));

        public Task<UserDetailResult?> GetUserAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<UserDetailResult?>(new UserDetailResult(
                Id: userId,
                Email: "user@example.com",
                DisplayName: "User",
                IsActive: true,
                IsLockedOut: false,
                LockoutEndUtc: null,
                FailedLoginAttempts: 0,
                Roles: [Role.WellKnownNames.User],
                CreatedAtUtc: DateTime.UtcNow,
                UpdatedAtUtc: DateTime.UtcNow));

        public Task<IReadOnlyList<RoleResult>> ListRolesAsync(CancellationToken cancellationToken) =>
            Task.FromResult((IReadOnlyList<RoleResult>)[]);

        public Task<IReadOnlyList<PermissionResult>> ListPermissionsAsync(CancellationToken cancellationToken) =>
            Task.FromResult((IReadOnlyList<PermissionResult>)[]);
    }
}
