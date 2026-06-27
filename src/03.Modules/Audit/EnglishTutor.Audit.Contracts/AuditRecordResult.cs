namespace EnglishTutor.Audit.Contracts;

public sealed record AuditRecordResult(
    bool IsSuccess,
    string? ErrorCode = null,
    string? ErrorMessage = null);
