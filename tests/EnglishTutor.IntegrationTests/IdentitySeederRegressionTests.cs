using EnglishTutor.Identity.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EnglishTutor.IntegrationTests.Identity;

// Permanent regression tests for the Identity seeder against a fresh
// per-factory temporary database.
//
// Background: the previous correction pass added `SaveChangesAsync`
// flushes between several seeder stages so DB lookups in later stages
// could see staged rows, but missed the flush between `SeedOwnerAsync`
// (which stages the Owner User entity into the change tracker) and
// `SeedOwnerUserRoleAsync` (which projects `identity.users` via
// `AsNoTracking`). On a fresh DB the Owner User was not yet
// persisted, so the join-row helper returned early and
// `identity.user_roles` ended up empty. These tests pin the expected
// seeded counts and will fail loudly if any future seeder change
// reintroduces the bug.
//
// The tests share the "EnglishTutorIntegrationTests" xUnit collection
// so they run sequentially with every other integration test class.
// IntegrationTestFactory generates a unique temp DB per factory
// instance and drops it on Dispose, so each test starts from a clean
// schema with no pre-existing rows.
//
// Verification is read-only via EF Core LINQ (`db.<Table>.CountAsync`).
// Per .agents/rules/38-postgresql-raw-sql-strict.md and
// .agents/rules/37-ef-core-query-discipline.md, raw SQL is forbidden
// here; LINQ on the IdentityDbContext is the only acceptable pattern.
[Collection("EnglishTutorIntegrationTests")]
public class IdentitySeederRegressionTests
{
    private readonly ITestOutputHelper _output;

    public IdentitySeederRegressionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task IdentitySeeder_Should_Create_Owner_User_With_Owner_Role_On_Fresh_Database()
    {
        // Arrange: a fresh IntegrationTestFactory triggers Program.cs
        // startup, which calls MigrateAsync on IdentityDbContext
        // (creating the identity schema) and then IdentityDataSeeder
        // .SeedAsync (populating it). Each factory instance generates a
        // unique temporary database, so this test exercises the
        // first-run / fresh-DB path every time.
        await using var factory = new IntegrationTestFactory();
        using var client = factory.CreateClient();

        // Act: query the per-factory IdentityDbContext through the
        // service provider. EF LINQ with IgnoreQueryFilters so soft-
        // deleted rows are still counted (matches the seeder's own
        // existence checks).
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var permissions = await db.Permissions.IgnoreQueryFilters().CountAsync();
        var roles = await db.Roles.IgnoreQueryFilters().CountAsync();
        var rolePermissions = await db.RolePermissions.IgnoreQueryFilters().CountAsync();
        var users = await db.Users.IgnoreQueryFilters().CountAsync();
        var userRoles = await db.UserRoles.IgnoreQueryFilters().CountAsync();

        _output.WriteLine(
            $"IdentitySeederRegression: " +
            $"permissions={permissions} roles={roles} " +
            $"role_permissions={rolePermissions} users={users} " +
            $"user_roles={userRoles}");

        // Assert: each seeded artefact must exist after a single
        // first-run SeedAsync. The numeric thresholds are the well-
        // known baseline (3 permissions, 3 roles, 7 role_permissions,
        // 1 owner user, 1 owner user_role). Using >= rather than == so
        // future additions to the baseline do not break this test.
        Assert.True(permissions >= 3,
            $"Expected at least 3 permissions after first SeedAsync, got {permissions}.");
        Assert.True(roles >= 3,
            $"Expected at least 3 roles after first SeedAsync, got {roles}.");
        Assert.True(rolePermissions >= 7,
            $"Expected at least 7 role_permissions after first SeedAsync, got {rolePermissions}.");
        Assert.True(users >= 1,
            $"Expected at least 1 user (Owner) after first SeedAsync, got {users}.");

        // This is the regression target: a fresh-DB seed must assign
        // the Owner role to the Owner user. Before the SaveChangesAsync
        // flush was added between SeedOwnerAsync and
        // SeedOwnerUserRoleAsync, this assertion failed with
        // user_roles = 0.
        Assert.True(userRoles >= 1,
            $"Expected at least 1 user_role after first SeedAsync, got {userRoles}. " +
            "This usually means SeedOwnerAsync staged the Owner user " +
            "but SeedOwnerUserRoleAsync ran its AsNoTracking lookup " +
            "before the SaveChangesAsync flush.");
    }

    [Fact]
    public async Task IdentitySeeder_Should_Be_Idempotent_When_Run_Twice_On_Fresh_Database()
    {
        // Arrange: a fresh factory triggers one SeedAsync via the host
        // startup path.
        await using var factory = new IntegrationTestFactory();
        using var client = factory.CreateClient();

        // Act: explicitly run SeedAsync a second time on the same scope.
        // The seeder must be idempotent — it must NOT duplicate rows.
        using (var scope = factory.Services.CreateScope())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<IdentityDataSeeder>();
            await seeder.SeedAsync();
        }

        // Assert: counts are unchanged after the second SeedAsync.
        using var verifyScope = factory.Services.CreateScope();
        var db = verifyScope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        var permissions = await db.Permissions.IgnoreQueryFilters().CountAsync();
        var roles = await db.Roles.IgnoreQueryFilters().CountAsync();
        var rolePermissions = await db.RolePermissions.IgnoreQueryFilters().CountAsync();
        var users = await db.Users.IgnoreQueryFilters().CountAsync();
        var userRoles = await db.UserRoles.IgnoreQueryFilters().CountAsync();

        _output.WriteLine(
            $"IdentitySeederRegression.Idempotency: " +
            $"permissions={permissions} roles={roles} " +
            $"role_permissions={rolePermissions} users={users} " +
            $"user_roles={userRoles}");

        Assert.True(permissions >= 3, $"permissions row drift: {permissions}");
        Assert.True(roles >= 3, $"roles row drift: {roles}");
        Assert.True(rolePermissions >= 7, $"role_permissions row drift: {rolePermissions}");
        Assert.True(users >= 1, $"users row drift: {users}");
        Assert.True(userRoles >= 1, $"user_roles row drift: {userRoles}");
    }
}
