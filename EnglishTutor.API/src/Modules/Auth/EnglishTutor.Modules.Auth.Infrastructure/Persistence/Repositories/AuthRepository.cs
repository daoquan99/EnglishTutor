using EnglishTutor.Modules.Auth.Application.Abstractions;
using EnglishTutor.Modules.Auth.Application.Shared.DTOs;
using EnglishTutor.Modules.Auth.Domain.AuthPermission;
using EnglishTutor.Modules.Auth.Domain.AuthRole;
using EnglishTutor.Modules.Auth.Domain.AuthRole.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent;
using EnglishTutor.Modules.Auth.Domain.AuthSession;
using EnglishTutor.Modules.Auth.Domain.AuthSession.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser;
using EnglishTutor.Modules.Auth.Domain.AuthUser.Entities;
using EnglishTutor.Modules.Auth.Domain.AuthUser.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Modules.Auth.Infrastructure.Persistence.Repositories;

public sealed class AuthRepository(AuthDbContext dbContext) : IAuthRepository
{
    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken) =>
        dbContext.AuthUsers.AnyAsync(user => user.Email == email, cancellationToken);

    public Task<AuthUser?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.AuthUsers.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public Task<AuthUser?> GetByEmailAsync(Email email, CancellationToken cancellationToken) =>
        dbContext.AuthUsers.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<UserCredential?> GetCredentialByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.UserCredentials.SingleOrDefaultAsync(credential => credential.AuthUserId == userId, cancellationToken);

    public Task AddAsync(AuthUser user, UserCredential credential, CancellationToken cancellationToken)
    {
        dbContext.AuthUsers.Add(user);
        dbContext.UserCredentials.Add(credential);
        return Task.CompletedTask;
    }

    public Task<AuthUser?> GetByIdWithRolesAsync(Guid userId, CancellationToken cancellationToken) =>
        dbContext.AuthUsers
            .Include(user => user.Roles)
            .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

    public async Task<(IReadOnlyList<AuthUser> Items, int Total)> ListAsync(
        string? search,
        UserStatusFilter status,
        Guid? roleId,
        int page,
        int pageSize,
        UserSortOrder sort,
        CancellationToken cancellationToken)
    {
        var query = dbContext.AuthUsers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(user =>
                EF.Functions.ILike(user.DisplayName, pattern)
                || EF.Functions.ILike(user.Email.Value, pattern));
        }

        query = status switch
        {
            UserStatusFilter.Active => query.Where(user => user.IsActive),
            UserStatusFilter.Suspended => query.Where(user => !user.IsActive),
            _ => query,
        };

        if (roleId is { } filterRoleId)
        {
            query = query.Where(user => user.Roles.Any(role => role.RoleId == filterRoleId));
        }

        var total = await query.CountAsync(cancellationToken);

        var ordered = sort switch
        {
            UserSortOrder.OldestFirst => query.OrderBy(user => user.CreatedAtUtc).ThenBy(user => user.Id),
            UserSortOrder.RecentlyUpdated => query.OrderByDescending(user => user.UpdatedAtUtc).ThenBy(user => user.Id),
            UserSortOrder.DisplayNameAsc => query.OrderBy(user => user.DisplayName).ThenBy(user => user.Id),
            _ => query.OrderByDescending(user => user.CreatedAtUtc).ThenBy(user => user.Id),
        };

        var items = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(user => user.Roles)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
