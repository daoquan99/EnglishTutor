namespace EnglishTutor.Identity.Application.Abstractions;

public interface IPasswordHasher
{
    string HashPassword(string plaintextPassword);

    PasswordVerificationResult VerifyPassword(string plaintextPassword, string storedHash);
}

public enum PasswordVerificationResult
{
    Failed = 0,
    Success = 1,
    SuccessRehashNeeded = 2
}
