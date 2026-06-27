using Microsoft.AspNetCore.DataProtection;

namespace EnglishTutor.BuildingBlocks.Infrastructure.SecretProtection;

public sealed class DataProtectionSecretProtector : ISecretProtector
{
    private readonly IDataProtector _protector;

    public DataProtectionSecretProtector(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("EnglishTutor.Secrets");
    }

    public string Protect(string plaintext)
    {
        return _protector.Protect(plaintext);
    }

    public string Unprotect(string protectedValue)
    {
        return _protector.Unprotect(protectedValue);
    }
}
