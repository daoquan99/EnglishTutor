namespace EnglishTutor.BuildingBlocks.Infrastructure.SecretProtection;

public interface ISecretProtector
{
    string Protect(string plaintext);
    string Unprotect(string protectedValue);
}
