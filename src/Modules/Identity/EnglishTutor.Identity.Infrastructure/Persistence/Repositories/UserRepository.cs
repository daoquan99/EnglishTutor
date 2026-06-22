using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.Repositories;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EnglishTutor.Identity.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>. Uses the existing
/// <c>DbSet&lt;User&gt; Users</c> property. Email filtering uses the owned
/// navigation <c>u.Email.Value</c>; EF translates that to a SQL predicate
/// on the owned column, so the User.Email getter (which throws on
/// not-yet-initialised backing fields) is never invoked at query-compile
/// time. The result is then projected via <c>AsNoTracking</c> to avoid the
/// materialisation-time getter as well.
/// </summary>
internal sealed class UserRepository : IUserRepository
{
    private readonly IdentityDbContext _db;

    public UserRepository(IdentityDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByIdAsync(Guid userId, CancellationToken ct) =>
        GetByIdAsync(userId, includeDeleted: false, ct);

    public async Task<User?> GetByIdAsync(Guid userId, bool includeDeleted, CancellationToken ct)
    {
        IQueryable<User> query = _db.Users;
        if (includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }
        return await query.FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<User?> FindByEmailAsync(
        Email email, bool includeDeleted, CancellationToken ct)
    {
        IQueryable<User> query = _db.Users
            .AsNoTracking()
            .Where(u => u.Email.Value == email.Value);

        if (!includeDeleted)
        {
            query = query.IgnoreQueryFilters();
        }

        return await query.FirstOrDefaultAsync(ct);
    }

    public void Add(User user) => _db.Users.Add(user);
}
