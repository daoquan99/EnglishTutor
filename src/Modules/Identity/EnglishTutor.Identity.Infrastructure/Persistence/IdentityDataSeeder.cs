using EnglishTutor.BuildingBlocks.Application.DateTime;
using EnglishTutor.Identity.Application.Abstractions;
using EnglishTutor.Identity.Domain.Aggregates.Roles;
using EnglishTutor.Identity.Domain.Aggregates.Roles.Entities;
using EnglishTutor.Identity.Domain.Aggregates.Users;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using EnglishTutor.Identity.Infrastructure.Persistence.Seed.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnglishTutor.Identity.Infrastructure.Persistence;

/// <summary>
/// Seeds Owner / Admin / User roles + the Owner user account on first startup.
/// Idempotent — running it twice does not duplicate data.
/// </summary>
public sealed class IdentityDataSeeder
{
    private readonly IdentityDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly SeedDataOptions _options;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        IdentityDbContext db,
        IPasswordHasher passwordHasher,
        IOptions<SeedDataOptions> options,
        IDateTimeProvider clock,
        ILogger<IdentityDataSeeder> logger)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _options = options.Value;
        _clock = clock;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedRolesAsync(cancellationToken);
        await SeedOwnerAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken ct)
    {
        if (!await _db.Roles.IgnoreQueryFilters().AnyAsync(r => r.Name == Role.WellKnownNames.Owner, ct))
        {
            _db.Roles.Add(new Role(Role.WellKnownNames.Owner, "System Owner", priority: 100));
        }
        if (!await _db.Roles.IgnoreQueryFilters().AnyAsync(r => r.Name == Role.WellKnownNames.Admin, ct))
        {
            _db.Roles.Add(new Role(Role.WellKnownNames.Admin, "Administrator", priority: 50));
        }
        if (!await _db.Roles.IgnoreQueryFilters().AnyAsync(r => r.Name == Role.WellKnownNames.User, ct))
        {
            _db.Roles.Add(new Role(Role.WellKnownNames.User, "Standard User", priority: 10));
        }
    }

    private async Task SeedOwnerAsync(CancellationToken ct)
    {
        var ownerEmail = _options.Owner.Email.Trim().ToLowerInvariant();
        if (await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email.Value == ownerEmail, ct))
        {
            return;
        }

        // Dev default: random password (logged ONCE so dev can copy it). In production
        // the password MUST be supplied via SeedData:Owner:Password configuration.
        var password = string.IsNullOrWhiteSpace(_options.Owner.Password)
            ? GenerateRandomPassword()
            : _options.Owner.Password;

        var ownerRole = await _db.Roles.IgnoreQueryFilters().FirstAsync(r => r.Name == Role.WellKnownNames.Owner, ct);

        var user = User.Create(
            email: Email.Create(ownerEmail),
            passwordHash: HashedPassword.FromNewHash(_passwordHasher.HashPassword(password)),
            displayName: _options.Owner.DisplayName,
            roleIds: new[] { ownerRole.Id },
            createdByUserId: null);

        _db.Users.Add(user);

        _logger.LogWarning(
            "Seeded Owner account. Email: {Email}. Initial password: {Password}. " +
            "Rotate this password IMMEDIATELY in production environments.",
            ownerEmail, password);
    }

    private string GenerateRandomPassword()
    {
        const string chars = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var bytes = new byte[24];
        System.Security.Cryptography.RandomNumberGenerator.Fill(bytes);
        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}
