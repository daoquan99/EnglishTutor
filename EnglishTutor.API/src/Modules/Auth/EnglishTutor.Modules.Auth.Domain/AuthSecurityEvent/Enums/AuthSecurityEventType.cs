namespace EnglishTutor.Modules.Auth.Domain.AuthSecurityEvent.Enums;

public enum AuthSecurityEventType
{
    RefreshTokenHashMismatch,
    RefreshTokenMissingFromCache,
    RefreshTokenReuseDetected,
    SessionRevoked,
    AdminLockedAccount,
    AdminUserSuspended,
    AdminUserRestored,
    AdminUserUpdated,
    AdminUserRolesChanged
}
