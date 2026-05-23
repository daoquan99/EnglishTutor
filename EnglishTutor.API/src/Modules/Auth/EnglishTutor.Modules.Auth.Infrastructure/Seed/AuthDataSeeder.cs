using EnglishTutor.BuildingBlocks.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Contracts.Permissions;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;
using EnglishTutor.Modules.Auth.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EnglishTutor.Modules.Auth.Infrastructure.Seed;

public sealed class AuthDataSeeder(
    AuthDbContext dbContext,
    IPasswordHasher passwordHasher,
    IConfiguration configuration,
    IDateTimeProvider dateTimeProvider)
{
    private const string AdminRoleName = "admin";

    private static readonly IReadOnlyDictionary<string, string> PermissionDescriptions =
        new Dictionary<string, string>
        {
            [PermissionCodes.FullAccess] = "Full administrative access.",
            [PermissionCodes.AuthUsersRead] = "Read authentication users.",
            [PermissionCodes.AuthUsersManage] = "Manage authentication users (update profile, suspend, restore, assign roles).",
            [PermissionCodes.AuthSessionsRead] = "Read authentication sessions.",
            [PermissionCodes.AuthSecurityEventsRead] = "Read authentication security events.",
            [PermissionCodes.AuthSecurityEventsReview] = "Review authentication security events.",
            [PermissionCodes.AuthRolesRead] = "Read authentication roles.",
            [PermissionCodes.AuthRolesManage] = "Manage authentication roles.",
            [PermissionCodes.AuthPermissionsRead] = "Read authentication permissions.",
            [PermissionCodes.AuthPermissionsManage] = "Manage authentication permissions.",
            [PermissionCodes.UsersProfilesRead] = "Read user profiles.",
            [PermissionCodes.UsersProfilesManage] = "Manage user profiles.",
            [PermissionCodes.UsersLanguagesManage] = "Manage user language settings.",
            [PermissionCodes.VocabularyItemsRead] = "Read vocabulary items.",
            [PermissionCodes.VocabularyItemsManage] = "Manage vocabulary items.",
            [PermissionCodes.VocabularyReviewsRead] = "Read vocabulary review data.",
            [PermissionCodes.StudyPlansRead] = "Read study plans.",
            [PermissionCodes.StudyPlansManage] = "Manage study plans.",
            [PermissionCodes.LearningContentRead] = "Read learning content.",
            [PermissionCodes.LearningContentManage] = "Manage learning content.",
            [PermissionCodes.ExercisesRead] = "Read exercises.",
            [PermissionCodes.ExercisesManage] = "Manage exercises.",
            [PermissionCodes.SpeakingSessionsRead] = "Read speaking sessions.",
            [PermissionCodes.SpeakingSessionsManage] = "Manage speaking sessions.",
            [PermissionCodes.MistakesRead] = "Read mistakes.",
            [PermissionCodes.MistakesManage] = "Manage mistakes.",
            [PermissionCodes.AssessmentsRead] = "Read assessments.",
            [PermissionCodes.AssessmentsManage] = "Manage assessments.",
            [PermissionCodes.ProgressRead] = "Read progress.",
            [PermissionCodes.ProgressManage] = "Manage progress.",
            [PermissionCodes.NotificationsRead] = "Read notifications.",
            [PermissionCodes.NotificationsManage] = "Manage notifications.",
            [PermissionCodes.AiProvidersRead] = "Read AI providers.",
            [PermissionCodes.AiProvidersManage] = "Manage AI providers.",
            [PermissionCodes.AiRoutesRead] = "Read AI runtime routes.",
            [PermissionCodes.AiRoutesManage] = "Manage AI runtime routes.",
            [PermissionCodes.AiPromptsRead] = "Read AI prompt templates.",
            [PermissionCodes.AiPromptsManage] = "Manage AI prompt templates.",
            [PermissionCodes.AiLogsRead] = "Read AI request logs.",
            [PermissionCodes.ReportsRead] = "Read reports.",
            [PermissionCodes.ReportsManage] = "Manage reports."
        };

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = dateTimeProvider.UtcNow;
        var permissions = await SeedPermissionsAsync(utcNow, cancellationToken);
        var adminRole = await SeedAdminRoleAsync(utcNow, cancellationToken);
        await SeedRolePermissionsAsync(adminRole.Id, permissions.Values.Select(permission => permission.Id).ToList(), utcNow, cancellationToken);
        var adminUser = await SeedAdminUserAsync(utcNow, cancellationToken);
        await SeedUserRoleAsync(adminUser.Id, adminRole.Id, utcNow, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Dictionary<string, AuthPermission>> SeedPermissionsAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var permissions = await dbContext.AuthPermissions
            .ToDictionaryAsync(permission => permission.Code, cancellationToken);

        foreach (var code in PermissionCodes.All)
        {
            var description = PermissionDescriptions[code];
            if (permissions.TryGetValue(code, out var existing))
            {
                existing.UpdateDescription(description, utcNow);
                // Do NOT call Enable here: operator-disabled permissions must stay disabled across restarts.
                continue;
            }

            var permission = AuthPermission.Create(code, description, utcNow);
            dbContext.AuthPermissions.Add(permission);
            permissions[code] = permission;
        }

        return permissions;
    }

    private async Task<AuthRole> SeedAdminRoleAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var role = await dbContext.AuthRoles
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(
                candidate => candidate.Name == AdminRoleName,
                cancellationToken);

        if (role is not null)
        {
            role.Restore();
            role.UpdateDescription("System administrator role with all permissions.", utcNow);
            // Do NOT call Enable here: operator-disabled admin role state must persist across restarts.
            return role;
        }

        role = AuthRole.Create(AdminRoleName, "System administrator role with all permissions.", isSystem: true, utcNow);
        dbContext.AuthRoles.Add(role);
        return role;
    }

    private async Task SeedRolePermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<Guid> permissionIds,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        var existingPermissionIds = await dbContext.AuthRolePermissions
            .Where(rolePermission => rolePermission.RoleId == roleId)
            .Select(rolePermission => rolePermission.PermissionId)
            .ToListAsync(cancellationToken);

        foreach (var permissionId in permissionIds.Except(existingPermissionIds))
        {
            dbContext.AuthRolePermissions.Add(AuthRolePermission.Create(roleId, permissionId, utcNow));
        }
    }

    private async Task<AuthUser> SeedAdminUserAsync(DateTime utcNow, CancellationToken cancellationToken)
    {
        var email = Email.Create(configuration["SeedData:Admin:Email"] ?? "admin@englishtutor.local");
        var displayName = configuration["SeedData:Admin:DisplayName"] ?? "System Admin";
        var password = configuration["SeedData:Admin:Password"];
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("SeedData:Admin:Password must be configured when seed data is enabled.");
        }

        var user = await dbContext.AuthUsers
            .IgnoreQueryFilters()
            .SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);
        if (user is not null)
        {
            user.Restore();
            user.Activate();
            var existingCredential = await dbContext.UserCredentials.SingleOrDefaultAsync(
                credential => credential.AuthUserId == user.Id,
                cancellationToken);

            if (existingCredential is null)
            {
                // Recover from a corrupted state where the user row exists but the credential row was deleted.
                dbContext.UserCredentials.Add(UserCredential.Create(user.Id, passwordHasher.Hash(password), utcNow));
            }
            // Do NOT overwrite an existing credential with the configured password: an admin who rotates
            // the password in-app must not have it silently reset on the next deployment. Operators that
            // need to recover access can clear the UserCredentials row to trigger re-creation above.

            return user;
        }

        user = AuthUser.Register(email, displayName, utcNow);
        var credential = UserCredential.Create(user.Id, passwordHasher.Hash(password), utcNow);
        dbContext.AuthUsers.Add(user);
        dbContext.UserCredentials.Add(credential);
        return user;
    }

    private async Task SeedUserRoleAsync(Guid userId, Guid roleId, DateTime utcNow, CancellationToken cancellationToken)
    {
        var exists = await dbContext.AuthUserRoles.AnyAsync(
            userRole => userRole.AuthUserId == userId && userRole.RoleId == roleId,
            cancellationToken);

        if (!exists)
        {
            dbContext.AuthUserRoles.Add(AuthUserRole.Create(userId, roleId, utcNow));
        }
    }
}
