namespace EnglishTutor.Modules.Auth.Domain.Enums;

public enum AuthSecurityEventType
{
    RefreshTokenHashMismatch,
    RefreshTokenMissingFromCache,
    RefreshTokenReuseDetected,
    SessionRevoked,
    AdminLockedAccount
}
