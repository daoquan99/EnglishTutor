using EnglishTutor.BuildingBlocks.Domain.Aggregates;
using EnglishTutor.BuildingBlocks.Domain.DomainEvents;
using EnglishTutor.Identity.Domain.Aggregates.Users.Events;
using EnglishTutor.Identity.Domain.Aggregates.Users.ValueObjects;
using EnglishTutor.Identity.Domain.Aggregates.Sessions.Events;

namespace EnglishTutor.Identity.Domain.Aggregates.Users;

/// <summary>
/// User aggregate root. Owns authentication state (password hash, lockout
/// status, failed-attempt counter) and references to assigned roles.
/// </summary>
/// <remarks>
/// <para>Auth-relevant mutations go through domain methods
/// (<see cref="VerifyPassword"/>, <see cref="RecordFailedLogin"/>,
/// <see cref="Lockout"/>) so invariants stay inside the aggregate.</para>
/// <para>Audit + soft-delete fields come from <see cref="AggregateRoot"/>
/// (per Update Pack).</para>
/// </remarks>
public sealed class User : AggregateRoot
{
    private readonly List<Guid> _roleIds = [];

    // Email + PasswordHash are owned value objects; EF Core maps them to
    // columns via the .OwnsOne() configuration. The backing fields are
    // initialised so EF Core's value-object comparer does not choke on
    // freshly-constructed entities.
    private Email? _emailBacking;
    private HashedPassword? _passwordHashBacking;

    public Email Email
    {
        get => _emailBacking ?? throw new InvalidOperationException("Email not initialized.");
        private set => _emailBacking = value;
    }

    public HashedPassword PasswordHash
    {
        get => _passwordHashBacking ?? throw new InvalidOperationException("PasswordHash not initialized.");
        private set => _passwordHashBacking = value;
    }

    public string DisplayName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public bool IsLockedOut { get; private set; }
    public DateTime? LockoutEndUtc { get; private set; }
    public int FailedLoginAttempts { get; private set; }

    /// <summary>Snapshot of assigned roles for fast read.</summary>
    public IReadOnlyList<Guid> RoleIds => _roleIds.AsReadOnly();

    private User() { } // EF Core

    /// <summary>
    /// Factory for new users. Generates a domain event for the Audit module.
    /// </summary>
    public static User Create(
        Email email,
        HashedPassword passwordHash,
        string displayName,
        IEnumerable<Guid> roleIds,
        Guid? createdByUserId)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(passwordHash);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? email.Value : displayName.Trim(),
            PasswordHash = passwordHash,
            IsActive = true,
            FailedLoginAttempts = 0
        };
        user._roleIds.AddRange(roleIds ?? Array.Empty<Guid>());

        user.RaiseDomainEvent(new UserCreatedDomainEvent(
            user.Id, user.Email.Value, createdByUserId));

        return user;
    }

    public void AssignRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role id is required.", nameof(roleId));
        }
        if (!_roleIds.Contains(roleId))
        {
            _roleIds.Add(roleId);
        }
    }

    public void RevokeRole(Guid roleId)
    {
        _roleIds.Remove(roleId);
    }

    public void Activate()
    {
        if (IsActive) return;
        IsActive = true;
    }

    public void Deactivate()
    {
        if (!IsActive) return;
        IsActive = false;
    }

    /// <summary>
    /// Verifies the supplied plaintext password. Returns false on
    /// mismatch and increments <see cref="FailedLoginAttempts"/>; returns
    /// true on success and resets the counter.
    /// Locked-out users always fail verification until lockout expires.
    /// </summary>
    public bool VerifyPassword(
        string plaintextPassword,
        Func<string, string, bool> verify,
        int maxFailedLoginAttempts,
        TimeSpan lockoutDuration)
    {
        ArgumentNullException.ThrowIfNull(verify);
        if (maxFailedLoginAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFailedLoginAttempts));
        }

        if (lockoutDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lockoutDuration));
        }

        if (!IsActive)
        {
            return false;
        }

        if (IsLockedOut && LockoutEndUtc > DateTime.UtcNow)
        {
            return false;
        }

        // Auto-unlock if lockout expired.
        if (IsLockedOut && LockoutEndUtc <= DateTime.UtcNow)
        {
            Unlock();
        }

        var ok = verify(plaintextPassword, PasswordHash.Hash);
        if (ok)
        {
            FailedLoginAttempts = 0;
            return true;
        }

        RecordFailedLogin(maxFailedLoginAttempts, lockoutDuration);
        return false;
    }

    /// <summary>
    /// Returns true only when this user is currently allowed to exchange a
    /// refresh token for new credentials. A deleted, deactivated, or actively
    /// locked-out account must never be issued a new access token even if it
    /// still holds a valid refresh token (Batch R1, H-04 /
    /// <c>security-identity.md</c>: "Deleted, disabled, locked, or inactive
    /// users must not refresh").
    /// </summary>
    /// <param name="nowUtc">Caller-supplied UTC time (keeps the aggregate
    /// deterministic and free of ambient clock reads).</param>
    public bool CanRefreshCredentials(DateTime nowUtc)
    {
        if (IsDeleted) return false;
        if (!IsActive) return false;
        if (IsLockedOut && (LockoutEndUtc is null || LockoutEndUtc > nowUtc)) return false;
        return true;
    }

    public void RecordFailedLogin(int maxFailedLoginAttempts, TimeSpan lockoutDuration)
    {
        if (maxFailedLoginAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxFailedLoginAttempts));
        }

        if (lockoutDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lockoutDuration));
        }

        FailedLoginAttempts++;
        if (FailedLoginAttempts >= maxFailedLoginAttempts)
        {
            Lockout(DateTime.UtcNow + lockoutDuration);
        }
    }

    public void Lockout(DateTime lockoutEndUtc)
    {
        IsLockedOut = true;
        LockoutEndUtc = lockoutEndUtc;
        RaiseDomainEvent(new UserLockedOutDomainEvent(Id, lockoutEndUtc));
    }

    public void Unlock()
    {
        IsLockedOut = false;
        LockoutEndUtc = null;
        FailedLoginAttempts = 0;
    }

    public void ChangePassword(HashedPassword newHash)
    {
        ArgumentNullException.ThrowIfNull(newHash);
        PasswordHash = newHash;
        FailedLoginAttempts = 0;
        IsLockedOut = false;
        LockoutEndUtc = null;
        RaiseDomainEvent(new PasswordChangedDomainEvent(Id));
    }

    public void ChangeDisplayName(string newDisplayName)
    {
        if (string.IsNullOrWhiteSpace(newDisplayName))
        {
            throw new ArgumentException("Display name is required.", nameof(newDisplayName));
        }
        DisplayName = newDisplayName.Trim();
    }
}
